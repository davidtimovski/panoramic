using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Services;
using Panoramic.Services.Storage;

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

        var notePath = data.RelativeFilePath is null ? null : Path.Combine(_storageService.StoragePath, data.RelativeFilePath);
        SetSelectedNote(notePath);
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public FileSystemItemPath? NotePath { get; private set; }
    public ExplorerItem? SelectedNote { get; private set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Type = WidgetType.Note,
            Area = Area,
            RelativeFilePath = NotePath?.Relative
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

    public IReadOnlyList<ExplorerItem> GetExplorerItems()
        => ConvertToExplorerItems(_storageService.FileSystemItems);

    public void SetSelectedNote(string? notePath)
    {
        var previousFilePath = NotePath?.Absolute;

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
                SelectedNote.Text = File.ReadAllText(NotePath.Absolute);
                _storageService.SelectNote(Id, previousFilePath, NotePath?.Absolute);
            }
            else
            {
                NotePath = null;
            }
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

    private List<ExplorerItem> ConvertToExplorerItems(IReadOnlyList<FileSystemItem> fileSystemItems)
        => fileSystemItems.Select(x =>
            new ExplorerItem(ConvertToExplorerItems(x.Children), x.SelectedInWidgetId is null || x.SelectedInWidgetId == Id)
            {
                Name = x.Name,
                Type = x.Type,
                Path = x.Path,
            }
        ).ToList();

    private ExplorerItem? GetSelectedNote(IReadOnlyList<FileSystemItem> fileSystemItems)
    {
        foreach (var item in fileSystemItems)
        {
            if (item.Type == FileType.File && item.Path.Equals(NotePath))
            {
                return new ExplorerItem(item.SelectedInWidgetId is null || item.SelectedInWidgetId == Id)
                {
                    Name = item.Name,
                    Type = item.Type,
                    Path = item.Path,
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
}
