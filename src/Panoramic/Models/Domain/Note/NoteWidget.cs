using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Services;
using Panoramic.Services.Storage;

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
        _storageService.FileCreated += FileCreated;

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
        _storageService.FileCreated += FileCreated;

        _dataFileName = $"{data.Id}.json";

        Id = data.Id;
        Area = data.Area;
        FontFamily = data.FontFamily;
        FontSize = data.FontSize;

        var notePath = data.RelativeFilePath is null ? null : Path.Combine(_storageService.StoragePath, data.RelativeFilePath);
        SetSelectedNote(notePath);
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.Note;
    public Area Area { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public FileSystemItemPath? NotePath { get; private set; }
    public ExplorerItem? SelectedNote { get; private set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            FontFamily = FontFamily,
            FontSize = FontSize,
            RelativeFilePath = NotePath?.Relative
        };

    public void SetSelectedNote(string? notePath)
    {
        if (notePath is null)
        {
            NotePath = null;
            SelectedNote = null;
        }
        else
        {
            NotePath = new(notePath, _storageService.StoragePath);
            SelectedNote = GetSelectedNote(_storageService.FileSystemItems);

            if (SelectedNote is not null)
            {
                SelectedNote.InitializeContent(File.ReadAllText(NotePath.Absolute));
            }
            else
            {
                NotePath = null;
            }
        }
    }

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

    public IReadOnlyList<ExplorerItem> GetExplorerItems()
    {
        var items = DirectoryToTreeViewNode(_storageService.FileSystemItems, _storageService.StoragePath);

        var rootPath = new FileSystemItemPath(_storageService.StoragePath, _storageService.StoragePath);
        var root = new ExplorerItem(_storageService, Path.GetFileName(rootPath.Absolute), FileType.Folder, rootPath, items);

        return [root];
    }

    private List<ExplorerItem> DirectoryToTreeViewNode(IReadOnlyList<FileSystemItem> fileSystemItems, string currentPath)
    {
        var folders = fileSystemItems
            .Where(x => x.Type == FileType.Folder && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .Select(x => new ExplorerItem(_storageService, x.Name, FileType.Folder, x.Path, DirectoryToTreeViewNode(fileSystemItems, x.Path.Absolute)));

        var notes = fileSystemItems
            .Where(x => x.Type == FileType.Note && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
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
        var data = GetData();
        var json = JsonSerializer.Serialize(data, _storageService.SerializerOptions);

        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), json);
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }

    private ExplorerItem? GetSelectedNote(IReadOnlyList<FileSystemItem> fileSystemItems)
    {
        foreach (var item in fileSystemItems)
        {
            if (item.Type == FileType.Note && item.Path.Equals(NotePath))
            {
                return new ExplorerItem(_storageService, item.Name, item.Type, item.Path, [])
                {
                    IsEnabled = item.SelectedInWidgetId is null || item.SelectedInWidgetId == Id
                };
            }

            var found = GetSelectedNote(item.Children);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private void FileCreated(object? _, FileCreatedEventArgs e)
    {
        if (e.Type == FileType.Note && e.WidgetId == Id)
        {
            SetSelectedNote(e.Path);
        }
    }
}
