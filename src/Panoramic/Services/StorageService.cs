using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Windows.Storage;

namespace Panoramic.Services;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    string StoragePath { get; }
    Dictionary<Guid, IWidget> Widgets { get; }

    Task ReadAsync();
    Task WriteAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueWidgetWrite(Guid id);

    void DeleteWidget(IWidget widget);
    Task AddNewWidgetAsync(IWidget widget);
    Task SaveWidgetAsync(IWidget widget);

    void ChangeStoragePath(string storagePath);
}

public class StorageService : IStorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Stores the sections that have been changed and need to be written to disk.
    /// </summary>
    private readonly HashSet<Guid> _unsavedWidgets = [];

    public StorageService()
    {
        StoragePath = InitializeStoragePath();

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            var tasks = new List<Task>(_unsavedWidgets.Count);

            foreach (var id in _unsavedWidgets)
            {
                var widget = Widgets[id];
                tasks.Add(widget.WriteAsync(StoragePath, SerializerOptions));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _unsavedWidgets.Clear();
            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    public string StoragePath { get; private set; }

    public Dictionary<Guid, IWidget> Widgets { get; } = [];

    public async Task ReadAsync()
    {
        var widgetsDirectory = Path.Combine(StoragePath, "widgets");
        var widgetFilePaths = Directory.GetFiles(widgetsDirectory, "*.json");

        var tasks = widgetFilePaths.Select(ReadWidgetAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteAsync()
    {
        var widgetKvps = Widgets.Where(x => x.Value is not null).ToList();

        var writeTasks = new List<Task>(widgetKvps.Count);
        foreach (var widgetKvp in widgetKvps)
        {
            writeTasks.Add(widgetKvp.Value.WriteAsync(StoragePath, SerializerOptions));
        }

        await Task.WhenAll(writeTasks).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id)
    {
        _unsavedWidgets.Add(id);

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(IWidget widget)
    {
        widget.Delete(StoragePath);

        Widgets.Remove(widget.Id);

        WidgetRemoved?.Invoke(this, new WidgetRemovedEventArgs(widget.Id));
    }

    public async Task AddNewWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync(StoragePath, SerializerOptions);

        if (!Widgets.TryAdd(widget.Id, widget))
        {
            Widgets[widget.Id] = widget;
        }

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(widget.Id));
    }

    public async Task SaveWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync(StoragePath, SerializerOptions);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(widget.Id));
    }

    public void ChangeStoragePath(string storagePath)
    {
        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, true);
        }

        Directory.Move(StoragePath, storagePath);

        StoragePath = storagePath;
    }

    private async Task ReadWidgetAsync(string widgetFilePath)
    {
        var json = await File.ReadAllTextAsync(widgetFilePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        switch (type)
        {
            case WidgetType.Note:
                var noteWidget = await NoteWidget.LoadAsync(json, StoragePath, SerializerOptions);
                Widgets.Add(noteWidget.Id, noteWidget);
                break;
            case WidgetType.LinkCollection:
                var linkCollectionWidget = LinkCollectionWidget.Load(json, SerializerOptions);
                Widgets.Add(linkCollectionWidget.Id, linkCollectionWidget);
                break;
            case WidgetType.RecentLinks:
                var recentLinksWidget = RecentLinksWidget.Load(json, SerializerOptions);
                Widgets.Add(recentLinksWidget.Id, recentLinksWidget);
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }
    }

    private string InitializeStoragePath()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        object? storagePathValue = localSettings.Values[nameof(StoragePath)];

        if (storagePathValue is null)
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Panoramic");
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            var widgetsPath = Path.Combine(defaultPath, "widgets");
            if (!Directory.Exists(widgetsPath))
            {
                Directory.CreateDirectory(widgetsPath);
            }

            return defaultPath;
        }

        return (string)storagePathValue;
    }
}

public class WidgetUpdatedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}

public class WidgetRemovedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
