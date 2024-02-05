using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Services;

namespace Panoramic.Models.Domain.Note;

public class NoteWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new note widget.
    /// </summary>
    public NoteWidget(IStorageService storageService, Area area)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = $"{Id}.json";

        Type = WidgetType.Note;
        Area = area;
    }

    /// <summary>
    /// Constructs a note widget based on existing data.
    /// </summary>
    public NoteWidget(IStorageService storageService, NoteData data)
    {
        _storageService = storageService;
        _dataFileName = $"{data.Id}.json";

        Id = data.Id;
        Type = WidgetType.Note;
        Area = data.Area;

        SetSelectedNote(data.RelativeFilePath);
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public string? RelativeFilePath { get; private set; }
    public string? FilePath { get; private set; }
    public ExplorerItem? SelectedNote { get; private set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Type = WidgetType.Note,
            Area = Area,
            RelativeFilePath = RelativeFilePath
        };

    public static bool FolderCanBeCreated(string title, string directory)
    {
        if (string.Equals(title, "widgets", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parentDirectory = Path.GetDirectoryName(directory)!;
        var path = Path.Combine(parentDirectory, title);
        return !Directory.Exists(path);
    }

    public static bool NoteCanBeCreated(string title, string directory)
    {
        var parentDirectory = Path.GetDirectoryName(directory)!;
        var path = Path.Combine(parentDirectory, $"{title}.md");
        return !File.Exists(path);
    }

    public void SetSelectedNote(string? relativeFilePath)
    {
        RelativeFilePath = relativeFilePath;

        if (RelativeFilePath is null)
        {
            FilePath = null;
            SelectedNote = null;
        }
        else
        {
            FilePath = Path.Combine(_storageService.StoragePath, RelativeFilePath);
            SelectedNote = GetSelectedNote(_storageService.ExplorerItems);
            if (SelectedNote is null)
            {
                throw new InvalidOperationException($"Cannot load note at path: {FilePath}");
            }

            SelectedNote.Text = File.ReadAllText(FilePath);
        }
    }

    public static async Task<NoteWidget> LoadAsync(IStorageService storageService, string json)
    {
        var data = JsonSerializer.Deserialize<NoteData>(json, storageService.SerializerOptions)!;
        return new(storageService, data);
    }

    public async Task WriteAsync()
    {
        var data = GetData();
        var json = JsonSerializer.Serialize(data, _storageService.SerializerOptions);

        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), json);
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }

    private ExplorerItem? GetSelectedNote(IReadOnlyList<ExplorerItem> items)
    {
        foreach (var item in items)
        {
            if (item.Type == FileType.File && string.Equals(item.Path, FilePath, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            var found = GetSelectedNote(item.Children);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
