﻿using System;
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
    private const string DataFileName = "data.json";

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
                var widget = Widgets[id];
                tasks.Add(SaveWidgetAsync(widget));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    public string StoragePath { get; private set; }

    public Dictionary<Guid, IWidget> Widgets { get; } = new();

    public async Task ReadAsync()
    {
        var widgetDirs = Directory.GetDirectories(StoragePath);

        var tasks = widgetDirs.Select(ReadWidgetAsync);

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
        if (!_unsavedWidgets.Contains(id))
        {
            _unsavedWidgets.Add(id);
        }

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

        if (Widgets.ContainsKey(widget.Id))
        {
            Widgets[widget.Id] = widget;
        }
        else
        {
            Widgets.Add(widget.Id, widget);
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
        var widgetDirs = Directory.GetDirectories(StoragePath);

        foreach (var directory in widgetDirs)
        {
            var directoryName = new DirectoryInfo(directory).Name;
            Directory.CreateDirectory(Path.Combine(storagePath, directoryName));
            
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                File.Move(file, Path.Combine(storagePath, directoryName, fileName), true);
            }
        }

        Directory.Delete(StoragePath, true);

        StoragePath = storagePath;
    }

    private async Task ReadWidgetAsync(string widgetDirectory)
    {
        var dataFilePath = Path.Combine(widgetDirectory, DataFileName);
        var json = await File.ReadAllTextAsync(dataFilePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        switch (type)
        {
            case WidgetType.RecentLinks:
                var recentLinksWidget = RecentLinksWidget.Load(json, SerializerOptions);
                Widgets.Add(recentLinksWidget.Id, recentLinksWidget);
                break;
            case WidgetType.LinkCollection:
                var linkCollectionWidget = LinkCollectionWidget.Load(json, SerializerOptions);
                Widgets.Add(linkCollectionWidget.Id, linkCollectionWidget);
                break;
            case WidgetType.Note:
                var noteWidget = await NoteWidget.LoadAsync(json, widgetDirectory, SerializerOptions);
                Widgets.Add(noteWidget.Id, noteWidget);
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
            return defaultPath;
        }

        return (string)storagePathValue;
    }
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