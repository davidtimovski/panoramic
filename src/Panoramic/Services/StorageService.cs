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
using Panoramic.Models.Domain.RecentLinks;
using Windows.Storage;

namespace Panoramic.Services;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    string StoragePath { get; }
    Dictionary<Guid, Widget> Widgets { get; }

    Task ReadAsync();
    Task WriteAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueWidgetWrite(Guid id);

    void DeleteWidget(Guid id);
    Task AddNewWidgetAsync<T>(T widget)
        where T : Widget;
    Task SaveWidgetAsync(Guid id);

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
    private readonly HashSet<Guid> _unsavedWidgets = new();

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
                tasks.Add(SaveWidgetAsync(id));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    public string StoragePath { get; private set; }

    public Dictionary<Guid, Widget> Widgets { get; } = new();

    public async Task ReadAsync()
    {
        var dataFiles = Directory.GetFiles(StoragePath, "*.json");

        var tasks = dataFiles.Select(ReadWidgetDataAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteAsync()
    {
        var widgetKvps = Widgets.Where(x => x.Value is not null).ToList();

        var writeTasks = new List<Task>(widgetKvps.Count);
        foreach (var widgetKvp in widgetKvps)
        {
            writeTasks.Add(SaveWidgetAsync(widgetKvp.Key));
        }

        await Task.WhenAll(writeTasks).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id)
    {
        if (!_unsavedWidgets.Contains(id))
        {
            _unsavedWidgets.Add(id);
        }

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(Guid id)
    {
        Widgets.Remove(id);
        File.Delete(GetWritePath(id));

        WidgetRemoved?.Invoke(this, new WidgetRemovedEventArgs(id));
    }

    public async Task AddNewWidgetAsync<T>(T widget)
        where T : Widget
    {
        if (Widgets.ContainsKey(widget.Id))
        {
            Widgets[widget.Id] = widget;
        }
        else
        {
            Widgets.Add(widget.Id, widget);
        }

        var json = Serialize(widget);
        await File.WriteAllTextAsync(GetWritePath(widget.Id), json);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(widget.Id));
    }

    public async Task SaveWidgetAsync(Guid id)
    {
        var json = Serialize(Widgets[id]);

        await File.WriteAllTextAsync(GetWritePath(id), json);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(id));
    }

    public void ChangeStoragePath(string storagePath)
    {
        var dataFiles = Directory.GetFiles(StoragePath, "*.json");

        foreach (var file in dataFiles)
        {
            var fileName = Path.GetFileName(file);
            File.Move(file, Path.Combine(storagePath, fileName));
        }

        StoragePath = storagePath;
    }

    private async Task ReadWidgetDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        Widget data = type switch
        {
            WidgetType.RecentLinks => RecentLinksWidget.Load(json, SerializerOptions),
            WidgetType.LinkCollection => LinkCollectionWidget.Load(json, SerializerOptions),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        Widgets.Add(data.Id, data);
    }

    private static string Serialize(Widget widget)
    {
        return widget.Type switch
        {
            WidgetType.RecentLinks => ((RecentLinksWidget)widget).Serialize(SerializerOptions),
            WidgetType.LinkCollection => ((LinkCollectionWidget)widget).Serialize(SerializerOptions),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };
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
            return defaultPath;
        }

        return (string)storagePathValue;
    }

    private string GetWritePath(Guid id) => Path.Combine(StoragePath, $"{id}.json");
}

public class WidgetUpdatedEventArgs : EventArgs
{
    public WidgetUpdatedEventArgs(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}

public class WidgetRemovedEventArgs : EventArgs
{
    public WidgetRemovedEventArgs(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
