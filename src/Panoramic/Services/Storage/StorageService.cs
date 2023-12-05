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

namespace Panoramic.Services.Storage;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    Dictionary<string, WidgetData> Sections { get; }

    Task ReadAsync();
    Task WriteAsync();

    /// <summary>
    /// Schedules a section write to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueSectionWrite(string section);

    void DeleteWidget(string section);
    Task AddNewWidgetAsync<T>(string section, T data)
        where T : WidgetData;
    Task SaveWidgetAsync<T>(string section)
        where T : WidgetData;
}

public class StorageService : IStorageService
{
    private static readonly string BasePath = "C:\\Users\\david\\Desktop\\overview";
    private static readonly IReadOnlyDictionary<string, string> DataPaths = new Dictionary<string, string>
    {
        { "A1", Path.Combine(BasePath, "A1.json") },
        { "A2", Path.Combine(BasePath, "A2.json") },
        { "A3", Path.Combine(BasePath, "A3.json") },
        { "B1", Path.Combine(BasePath, "B1.json") },
        { "B2", Path.Combine(BasePath, "B2.json") },
        { "B3", Path.Combine(BasePath, "B3.json") },
        { "C1", Path.Combine(BasePath, "C1.json") },
        { "C2", Path.Combine(BasePath, "C2.json") },
        { "C3", Path.Combine(BasePath, "C3.json") },
        { "D1", Path.Combine(BasePath, "D1.json") },
        { "D2", Path.Combine(BasePath, "D2.json") }
    };
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
    private readonly HashSet<string> _sectionsToWrite = new();

    public StorageService()
    {
        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            var tasks = new List<Task>(_sectionsToWrite.Count);

            foreach (var section in _sectionsToWrite)
            {
                tasks.Add(WriteSectionAsync(section));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    public Dictionary<string, WidgetData> Sections { get; } = new();

    public async Task ReadAsync()
    {
        var dataFiles = Directory.GetFiles(BasePath, "*.json");

        var tasks = dataFiles.Select(ReadWidgetDataAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteAsync()
    {
        var usedSections = Sections.Where(x => x.Value is not null).ToList();

        var writeTasks = new List<Task>(usedSections.Count);
        foreach (var section in usedSections)
        {
            writeTasks.Add(WriteSectionAsync(section.Key));
        }

        await Task.WhenAll(writeTasks).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void EnqueueSectionWrite(string section)
    {
        if (!_sectionsToWrite.Contains(section))
        {
            _sectionsToWrite.Add(section);
        }

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(string section)
    {
        Sections.Remove(section);
        File.Delete(DataPaths[section]);

        WidgetRemoved?.Invoke(this, new WidgetRemovedEventArgs(section));
    }

    public async Task AddNewWidgetAsync<T>(string section, T data)
        where T : WidgetData
    {
        if (Sections.ContainsKey(section))
        {
            Sections[section] = data;
        }
        else
        {
            Sections.Add(section, data);
        }

        var json = JsonSerializer.Serialize(data, SerializerOptions);
        await File.WriteAllTextAsync(DataPaths[section], json);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(section));
    }

    public async Task SaveWidgetAsync<T>(string section)
        where T : WidgetData
    {
        var json = JsonSerializer.Serialize((T)Sections[section], SerializerOptions);
        await File.WriteAllTextAsync(DataPaths[section], json);

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(section));
    }

    private async Task ReadWidgetDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var section = Path.GetFileNameWithoutExtension(filePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        WidgetData data = type switch
        {
            WidgetType.RecentLinks => JsonSerializer.Deserialize<RecentLinksWidgetData>(json, SerializerOptions)!,
            WidgetType.LinkCollection => JsonSerializer.Deserialize<LinkCollectionWidgetData>(json, SerializerOptions)!,
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        Sections.Add(section, data);
    }

    private Task WriteSectionAsync(string section)
    {
        var value = Sections[section]!;
        var path = DataPaths[section];

        var json = value.Type switch
        {
            WidgetType.RecentLinks => JsonSerializer.Serialize((RecentLinksWidgetData)value, SerializerOptions),
            WidgetType.LinkCollection => JsonSerializer.Serialize((LinkCollectionWidgetData)value, SerializerOptions),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        return File.WriteAllTextAsync(path, json);
    }
}

public class WidgetUpdatedEventArgs : EventArgs
{
    public WidgetUpdatedEventArgs(string section)
    {
        Section = section;
    }

    public string Section { get; }
}

public class WidgetRemovedEventArgs : EventArgs
{
    public WidgetRemovedEventArgs(string section)
    {
        Section = section;
    }

    public string Section { get; }
}
