﻿using System;
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
using Panoramic.Models.Domain.WebView;
using Panoramic.Services.Preferences;
using Panoramic.Services.Preferences.Models;
using Panoramic.Services.Storage.Models;
using Panoramic.Utils;
using Windows.Storage;

namespace Panoramic.Services.Storage;

/// <inheritdoc/>
public sealed class StorageService : IStorageService
{
    private static DateTime AutoSaveFirstEnqueued = DateTime.Now;

    private readonly IPreferencesService _preferencesService;

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Holds the widgets that have been changed and need to be written to disk.
    /// </summary>
    private readonly HashSet<Guid> _unsavedWidgets = [];

    public const string DefaultDirectoryName = "Panoramic";
    public const string SystemDirectoryName = ".panoramic";

    public StorageService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;

        StoragePath = InitializeStoragePath();

        if (!Directory.Exists(WidgetsFolderPath))
        {
            Directory.CreateDirectory(WidgetsFolderPath);
        }

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = _preferencesService.AutoSaveInterval;
        _timer.Tick += async (timer, _) =>
        {
            DebugLogger.Log($"Running auto-save for {_unsavedWidgets.Count} widgets..");

            await WriteUnsavedChangesAsync();
        };

        _preferencesService.Changed += PreferencesChanged;
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
        try
        {
            foreach (var widget in noteWidgets)
            {
                Widgets.Add(widget.Id, widget);
            }

            var widgetFilePaths = Directory.GetFiles(WidgetsFolderPath, "*.md");

            var tasks = widgetFilePaths.Select(ReadWidgetAsync);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    public async Task WriteUnsavedChangesAsync()
    {
        try
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
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id, string change)
    {
        try
        {
            DebugLogger.Log($"Enqueuing widget write: {id}. Reason: {change}.");

            if (_unsavedWidgets.Count == 0)
            {
                // If this is the first change to be enqueued. Save the time.
                AutoSaveFirstEnqueued = DateTime.Now;
            }

            _unsavedWidgets.Add(id);

            if (DateTime.Now - AutoSaveFirstEnqueued <= _preferencesService.AutoSaveMaxDelay)
            {
                _timer.Stop();
                _timer.Start();
            }
        }
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    public void DeleteWidget(IWidget widget)
    {
        try
        {
            if (_unsavedWidgets.Contains(widget.Id))
            {
                _unsavedWidgets.Remove(widget.Id);
            }

            widget.Delete();

            Widgets.Remove(widget.Id);

            WidgetDeleted?.Invoke(this, new WidgetDeletedEventArgs { Widget = widget });
        }
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    public async Task AddNewWidgetAsync(IWidget widget)
    {
        try
        {
            await widget.WriteAsync();

            if (!Widgets.TryAdd(widget.Id, widget))
            {
                Widgets[widget.Id] = widget;
            }

            WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
        }
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    public async Task SaveWidgetAsync(IWidget widget)
    {
        try
        {
            if (_unsavedWidgets.Contains(widget.Id))
            {
                _unsavedWidgets.Remove(widget.Id);
            }

            await widget.WriteAsync();

            WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
        }
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
    }

    public void ChangeStoragePath(string storagePath)
    {
        try
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
        catch (Exception ex)
        {
            throw new StorageException(ex);
        }
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
            case WidgetType.WebView:
                var webViewWidget = WebViewWidget.Load(this, relativeFilePath, markdown);
                Widgets.Add(webViewWidget.Id, webViewWidget);
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

    private void PreferencesChanged(object? _, PreferencesChangedEventArgs e)
    {
        _timer.Interval = e.AutoSaveInterval;

        if (_timer.IsRunning)
        {
            _timer.Stop();
            _timer.Start();
        }
    }
}
