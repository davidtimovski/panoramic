using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services.Storage.Models;
using Panoramic.Utils;
using Windows.Storage;

namespace Panoramic.Services.Storage;

/// <inheritdoc/>
public sealed class StorageService : IStorageService
{
    private const string DefaultDirectoryName = "Panoramic";
    private const string SystemDirectoryName = ".panoramic";

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Holds the widgets that have been changed and need to be written to disk.
    /// </summary>
    private readonly HashSet<Guid> _unsavedWidgets = [];

    public StorageService()
    {
        StoragePath = InitializeStoragePath();

        if (!Directory.Exists(WidgetsFolderPath))
        {
            Directory.CreateDirectory(WidgetsFolderPath);
        }

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            DebugLogger.Log($"Running auto-save for {_unsavedWidgets.Count} widgets..");

            await WriteUnsavedChangesAsync();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetDeletedEventArgs>? WidgetDeleted;
    public event EventHandler<EventArgs>? StoragePathChanged;

    public string StoragePath { get; private set; }
    public string SystemFolderPath => Path.Combine(StoragePath, SystemDirectoryName);
    public string WidgetsFolderPath => Path.Combine(StoragePath, SystemDirectoryName, "widgets");

    public Dictionary<Guid, IWidget> Widgets { get; } = [];

    public async Task ReadWidgetsAsync(IReadOnlyList<IWidget> noteWidgets)
    {
        foreach (var widget in noteWidgets)
        {
            Widgets.Add(widget.Id, widget);
        }

        var widgetFilePaths = Directory.GetFiles(WidgetsFolderPath, "*.md");

        var tasks = widgetFilePaths.Select(ReadWidgetAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteUnsavedChangesAsync()
    {
        _timer.Stop();

        var tasks = new List<Task>(_unsavedWidgets.Count);

        foreach (var id in _unsavedWidgets)
        {
            tasks.Add(Widgets[id].WriteAsync());
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _unsavedWidgets.Clear();
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id, string change)
    {
        DebugLogger.Log($"Enqueuing widget write: {id}. Reason: {change}.");

        _unsavedWidgets.Add(id);

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(IWidget widget)
    {
        if (_unsavedWidgets.Contains(widget.Id))
        {
            _unsavedWidgets.Remove(widget.Id);
        }

        widget.Delete();

        Widgets.Remove(widget.Id);

        WidgetDeleted?.Invoke(this, new WidgetDeletedEventArgs { Id = widget.Id });
    }

    public async Task AddNewWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync();

        if (!Widgets.TryAdd(widget.Id, widget))
        {
            Widgets[widget.Id] = widget;
        }

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
    }

    public async Task SaveWidgetAsync(IWidget widget)
    {
        if (_unsavedWidgets.Contains(widget.Id))
        {
            _unsavedWidgets.Remove(widget.Id);
        }

        await widget.WriteAsync();

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
    }

    public void ChangeStoragePath(string storagePath)
    {
        if (string.Equals(StoragePath, storagePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, true);
        }

        Directory.Move(StoragePath, storagePath);

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[nameof(StoragePath)] = storagePath;

        StoragePath = storagePath;

        StoragePathChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task ReadWidgetAsync(string filePath)
    {
        var type = WidgetUtil.GetType(filePath);
        if (type == WidgetType.Note)
        {
            // Skip Note widgets as they're read by INotesOrchestrator
            return;
        }

        var relativeFilePath = Path.GetRelativePath(StoragePath, filePath);
        var markdown = await File.ReadAllTextAsync(filePath);

        switch (type)
        {
            case WidgetType.LinkCollection:
                var linkCollectionWidget = LinkCollectionWidget.Load(this, relativeFilePath, markdown);
                Widgets.Add(linkCollectionWidget.Id, linkCollectionWidget);
                break;
            case WidgetType.RecentLinks:
                var recentLinksWidget = RecentLinksWidget.Load(this, relativeFilePath, markdown);
                Widgets.Add(recentLinksWidget.Id, recentLinksWidget);
                break;
            case WidgetType.Checklist:
                var checklistWidget = ChecklistWidget.Load(this, relativeFilePath, markdown);
                Widgets.Add(checklistWidget.Id, checklistWidget);
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
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultDirectoryName);
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            return defaultPath;
        }

        return (string)storagePathValue;
    }
}
