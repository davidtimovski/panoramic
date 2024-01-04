using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain.Note;

public class NoteWidget : IWidget
{
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

    /// <summary>
    /// Checks whether the file already exists on the file system and returns false if it does.
    /// </summary>
    public static bool CanBeCreated(string title, string storagePath)
    {
        var filePath = Path.Combine(storagePath, $"{title}.md");
        return !File.Exists(filePath);
    }

    public static async Task<NoteWidget> LoadAsync(string json, string storagePath, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<NoteData>(json, options)!;
        var text = await File.ReadAllTextAsync(Path.Combine(storagePath, $"{data.Title}.md"));

        return new(data, text);
    }

    public async Task WriteAsync(string storagePath, JsonSerializerOptions options)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");

        var data = GetData();
        var json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(Path.Combine(storagePath, $"{Title}.md"), Text);
        await File.WriteAllTextAsync(Path.Combine(widgetsDirectory, $"{Id}.json"), json);
    }

    public void Delete(string storagePath)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");

        File.Delete(Path.Combine(storagePath, $"{Title}.md"));
        File.Delete(Path.Combine(widgetsDirectory, $"{Id}.json"));
    }
}
