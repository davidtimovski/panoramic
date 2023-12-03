using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Storage;

public class StorageService
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

    public static readonly Dictionary<string, WidgetData> Frames = new();

    public static async Task ReadAsync()
    {
        var dataFiles = Directory.GetFiles(BasePath, "*.json");

        var tasks = dataFiles.Select(ReadWidgetDataAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public static async Task WriteAsync()
    {
        var usedFrames = Frames.Where(x => x.Value is not null).ToList();

        var writeTasks = new List<Task>(usedFrames.Count);
        foreach (var frame in usedFrames)
        {
            var value = Frames[frame.Key]!;
            var path = DataPaths[frame.Key];

            var json = value.Type switch
            {
                WidgetType.Sample => JsonSerializer.Serialize((SampleWidgetData)value, SerializerOptions),
                WidgetType.RecentLinks => JsonSerializer.Serialize((RecentLinksWidgetData)value, SerializerOptions),
                _ => throw new InvalidOperationException("Unsupported widget type")
            };

            writeTasks.Add(File.WriteAllTextAsync(path, json));
        }

        await Task.WhenAll(writeTasks).ConfigureAwait(false);
    }

    private static async Task ReadWidgetDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var frame = Path.GetFileNameWithoutExtension(filePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        WidgetData data = type switch
        {
            WidgetType.Sample => JsonSerializer.Deserialize<SampleWidgetData>(json, SerializerOptions)!,
            WidgetType.RecentLinks => JsonSerializer.Deserialize<RecentLinksWidgetData>(json, SerializerOptions)!,
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        Frames.Add(frame, data);
    }
}
