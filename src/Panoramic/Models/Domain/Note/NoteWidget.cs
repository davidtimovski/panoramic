using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain.Note;

public class NoteWidget : IWidget
{
    private readonly string dataFileName;

    private string fileName;
    
    /// <summary>
    /// Constructs a new note widget.
    /// </summary>
    public NoteWidget(Area area, string title)
    {
        Id = Guid.NewGuid();
        dataFileName = $"{Id}.json";
        fileName = $"{title}.md";

        Type = WidgetType.Note;
        Area = area;
        Title = title;
        Text = string.Empty;
    }

    /// <summary>
    /// Constructs a note widget based on existing data.
    /// </summary>
    public NoteWidget(NoteData data, string text, string fileName)
    {
        Id = data.Id;
        dataFileName = $"{Id}.json";
        this.fileName = fileName;

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

        var fileName = $"{data.Title}.md";
        var text = await File.ReadAllTextAsync(Path.Combine(storagePath, fileName));

        return new(data, text, fileName);
    }

    public async Task WriteAsync(string storagePath, JsonSerializerOptions options)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");

        var newFileName = $"{Title}.md";
        if (newFileName != fileName)
        {
            var originalPath = Path.Combine(storagePath, fileName);
            var newPath = Path.Combine(storagePath, newFileName);
            File.Move(originalPath, newPath, true);

            fileName = newFileName;
        }

        var data = GetData();
        var json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(Path.Combine(storagePath, fileName), Text);
        await File.WriteAllTextAsync(Path.Combine(widgetsDirectory, dataFileName), json);
    }

    public void Delete(string storagePath)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");

        File.Delete(Path.Combine(storagePath, fileName));
        File.Delete(Path.Combine(widgetsDirectory, dataFileName));
    }
}
