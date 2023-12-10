using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Panoramic.Models;
using Panoramic.Services.Storage.Models;
using Windows.Storage;

namespace Panoramic.Services.Storage;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    string StoragePath { get; }
    Dictionary<Guid, WidgetData> Widgets { get; }

    Task ReadAsync();
    Task WriteAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueWidgetWrite(Guid id);

    void DeleteWidget(Guid id);
    Task AddNewWidgetAsync<T>(T data)
        where T : WidgetData;
    Task SaveWidgetAsync<T>(Guid id)
        where T : WidgetData;

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
        InitializeStoragePath();

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            var tasks = new List<Task>(_unsavedWidgets.Count);

            foreach (var id in _unsavedWidgets)
            {
                tasks.Add(WriteWidgetAsync(id));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    public string StoragePath { get; private set; }

    public Dictionary<Guid, WidgetData> Widgets { get; } = new();

    public async Task ReadAsync()
    {
        var dataFiles = Directory.GetFiles(StoragePath, "*.json");

        var tasks = dataFiles.Select(ReadWidgetDataAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteAsync()
    {
        var usedAreas = Widgets.Where(x => x.Value is not null).ToList();

        var writeTasks = new List<Task>(usedAreas.Count);
        foreach (var area in usedAreas)
        {
            writeTasks.Add(WriteWidgetAsync(area.Key));
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

    public async Task AddNewWidgetAsync<T>(T data)
        where T : WidgetData
    {
        if (Widgets.ContainsKey(data.Id))
        {
            Widgets[data.Id] = data;
        }
        else
        {
            Widgets.Add(data.Id, data);
        }

        var json = JsonSerializer.Serialize(data, SerializerOptions);
        await File.WriteAllTextAsync(GetWritePath(data.Id), json);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(data.Id));
    }

    public async Task SaveWidgetAsync<T>(Guid id)
        where T : WidgetData
    {
        var json = JsonSerializer.Serialize((T)Widgets[id], SerializerOptions);
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

        WidgetData data = type switch
        {
            WidgetType.RecentLinks => JsonSerializer.Deserialize<RecentLinksWidgetData>(json, SerializerOptions)!,
            WidgetType.LinkCollection => JsonSerializer.Deserialize<LinkCollectionWidgetData>(json, SerializerOptions)!,
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        Widgets.Add(data.Id, data);
    }

    private Task WriteWidgetAsync(Guid id)
    {
        var value = Widgets[id]!;
        var path = GetWritePath(id);

        var json = value.Type switch
        {
            WidgetType.RecentLinks => JsonSerializer.Serialize((RecentLinksWidgetData)value, SerializerOptions),
            WidgetType.LinkCollection => JsonSerializer.Serialize((LinkCollectionWidgetData)value, SerializerOptions),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        return File.WriteAllTextAsync(path, json);
    }

    private void InitializeStoragePath()
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
            StoragePath = defaultPath;
        }
        else
        {
            StoragePath = (string)storagePathValue;
        }
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
