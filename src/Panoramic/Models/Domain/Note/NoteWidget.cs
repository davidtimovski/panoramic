using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.Note;

public sealed class NoteWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new note widget.
    /// </summary>
    public NoteWidget(IStorageService storageService, Area area, string fontFamily, double fontSize)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = $"{Id}.json";

        Area = area;
        FontFamily = fontFamily;
        FontSize = fontSize;
    }

    /// <summary>
    /// Constructs a note widget based on existing data.
    /// </summary>
    public NoteWidget(IStorageService storageService, NoteData data)
    {
        _storageService = storageService;

        _dataFileName = $"{data.Id}.json";

        Id = data.Id;
        Area = data.Area;
        FontFamily = data.FontFamily;
        FontSize = data.FontSize;

        var notePath = data.RelativeFilePath is null ? null : Path.Combine(_storageService.StoragePath, data.RelativeFilePath);
        if (notePath is not null)
        {
            NotePath = new(notePath, _storageService.StoragePath);
        }
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.Note;
    public Area Area { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public FileSystemItemPath? NotePath { get; set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            FontFamily = FontFamily,
            FontSize = FontSize,
            RelativeFilePath = NotePath?.Relative
        };

    public static bool FolderCanBeCreated(string title, string directory)
    {
        title = title.Trim();

        if (string.Equals(title, "widgets", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = Path.Combine(directory, title);
        return !Directory.Exists(path);
    }

    public static bool NoteCanBeCreated(string title, string directory)
    {
        title = title.Trim();

        var path = Path.Combine(directory, $"{title}.md");
        return !File.Exists(path);
    }

    public IReadOnlyList<ExplorerItem> GetExplorerItems()
        => DirectoryToTreeViewNode(_storageService.FileSystemItems, _storageService.StoragePath);

    private List<ExplorerItem> DirectoryToTreeViewNode(IReadOnlyList<FileSystemItem> fileSystemItems, string currentPath)
    {
        var folders = fileSystemItems
            .Where(x => x.Type == FileType.Folder && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExplorerItem(_storageService, x.Name, FileType.Folder, x.Path, DirectoryToTreeViewNode(fileSystemItems, x.Path.Absolute)));

        var notes = fileSystemItems
            .Where(x => x.Type == FileType.Note && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExplorerItem(_storageService, x.Name, FileType.Note, x.Path, [])
            {
                IsEnabled = x.SelectedInWidgetId is null || x.SelectedInWidgetId == Id
            });

        return folders.Concat(notes).ToList();
    }

    public static Task<NoteWidget> LoadAsync(IStorageService storageService, string json)
    {
        var data = JsonSerializer.Deserialize<NoteData>(json, storageService.SerializerOptions)!;
        return Task.FromResult<NoteWidget>(new(storageService, data));
    }

    public async Task WriteAsync()
    {
        DebugLogger.Log($"Writing {Type} widget with ID: {Id}");

        var data = GetData();
        var json = JsonSerializer.Serialize(data, _storageService.SerializerOptions);

        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), json);
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }
}
