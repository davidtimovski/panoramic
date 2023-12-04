using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
    void DeleteWidget(string section);
    Task AddRecentLinksWidgetAsync(string section, string title, int capacity, bool resetEveryDay);
    Task AddLinkCollectionWidgetAsync(string section, string title);
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
            var value = Sections[section.Key]!;
            var path = DataPaths[section.Key];

            var json = value.Type switch
            {
                WidgetType.RecentLinks => JsonSerializer.Serialize((RecentLinksWidgetData)value, SerializerOptions),
                WidgetType.LinkCollection => JsonSerializer.Serialize((LinkCollectionWidgetData)value, SerializerOptions),
                _ => throw new InvalidOperationException("Unsupported widget type")
            };

            writeTasks.Add(File.WriteAllTextAsync(path, json));
        }

        await Task.WhenAll(writeTasks).ConfigureAwait(false);
    }

    public void DeleteWidget(string section)
    {
        Sections.Remove(section);
        File.Delete(DataPaths[section]);

        WidgetRemoved?.Invoke(this, new WidgetRemovedEventArgs(section));
    }

    // TODO: Bug. Will reset data on update.
    public async Task AddRecentLinksWidgetAsync(string section, string title, int capacity, bool resetEveryDay)
    {
        var data = new RecentLinksWidgetData
        {
            Title = title,
            Capacity = capacity,
            ResetEveryDay = resetEveryDay
        };

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

    // TODO: Bug. Will reset data on update.
    public async Task AddLinkCollectionWidgetAsync(string section, string title)
    {
        var data = new LinkCollectionWidgetData
        {
            Title = title
        };

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
