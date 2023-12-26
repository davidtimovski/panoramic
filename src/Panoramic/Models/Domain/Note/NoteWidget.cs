using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain.Note;

public class NoteWidget : IWidget
{
    private const string ContentFileName = "content.txt";

    public const string DefaultTitle = "My note";

    /// <summary>
    /// Constructs a new note widget.
    /// </summary>
    public NoteWidget(Area area, string title)
    {
        Id = Guid.NewGuid();
        Type = WidgetType.Note;
        Area = area;
        Title = title;
        Text = string.Empty;
    }

    /// <summary>
    /// Constructs a note widget based on en existing one.
    /// </summary>
    public NoteWidget(NoteData data, string text)
    {
        Id = data.Id;
        Type = WidgetType.Note;
        Area = data.Area;
        Title = data.Title;
        Text = text;
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public string Title { get; set; }

    public string Text { get; set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Type = WidgetType.Note,
            Area = Area,
            Title = Title
        };

    public static async Task<NoteWidget> LoadAsync(string json, string path, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<NoteData>(json, options)!;
        var text = await File.ReadAllTextAsync(Path.Combine(path, ContentFileName));

        return new(data, text);
    }

    public async Task WriteAsync(string storagePath, JsonSerializerOptions options)
    {
        var directory = Path.Combine(storagePath, Id.ToString());
        Directory.CreateDirectory(directory);

        var data = GetData();
        var json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(Path.Combine(directory, ContentFileName), Text);
        await File.WriteAllTextAsync(Path.Combine(directory, "data.json"), json);
    }

    public void Delete(string storagePath)
    {
        var directory = Path.Combine(storagePath, Id.ToString());

        File.Delete(Path.Combine(directory, ContentFileName));
        File.Delete(Path.Combine(directory, "data.json"));
        Directory.Delete(directory);
    }
}
