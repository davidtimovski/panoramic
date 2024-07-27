using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Services.Notes;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.Note;

public sealed class NoteWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new note widget.
    /// </summary>
    public NoteWidget(
        IStorageService storageService,
        INotesOrchestrator notesOrchestrator,
        Area area,
        HighlightColor headerHighlight,
        string fontFamily,
        double fontSize,
        int recentNotesCapacity)
    {
        _storageService = storageService;
        _notesOrchestrator = notesOrchestrator;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.Note);

        Area = area;
        HeaderHighlight = headerHighlight;
        FontFamily = fontFamily;
        FontSize = fontSize;
        Editing = true;
        recentNotes = [];
        RecentNotesCapacity = recentNotesCapacity;
    }

    /// <summary>
    /// Constructs a note widget based on existing data.
    /// </summary>
    private NoteWidget(IStorageService storageService, INotesOrchestrator notesOrchestrator, NoteData data)
    {
        _storageService = storageService;
        _notesOrchestrator = notesOrchestrator;

        _dataFileName = WidgetUtil.CreateDataFileName(data.Id, WidgetType.Note);

        Id = data.Id;
        Area = data.Area;
        HeaderHighlight = data.HeaderHighlight;
        FontFamily = data.FontFamily;
        FontSize = data.FontSize;
        Editing = data.Editing;
        recentNotes = data.RecentNotes.Select(x => new FileSystemItemPath(Path.Combine(_storageService.StoragePath, x), _storageService.StoragePath)).ToList();
        RecentNotesCapacity = data.RecentNotesCapacity;

        var notePath = data.RelativeFilePath is null ? null : Path.Combine(_storageService.StoragePath, data.RelativeFilePath);
        if (notePath is not null)
        {
            NotePath = new(notePath, _storageService.StoragePath);
        }
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.Note;
    public Area Area { get; set; }
    public HighlightColor HeaderHighlight { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }

    private FileSystemItemPath? notePath;
    public FileSystemItemPath? NotePath
    {
        get => notePath;
        set
        {
            var recent = recentNotes.ToList();
            if (NotePath is not null)
            {
                recent.Insert(0, NotePath);
            }

            var previousPath = NotePath;
            notePath = value;

            recentNotes.Clear();
            var mostRecentThree = recent.Where(x => !x.Equals(value)).Take(RecentNotesCapacity);
            recentNotes.AddRange(mostRecentThree);
        }
    }

    public bool Editing { get; set; }

    private readonly List<FileSystemItemPath> recentNotes;

    /// <summary>
    /// Recently opened notes, excluding notes that are currently open across all Note widgets in the UI.
    /// </summary>
    public List<FileSystemItemPath> RecentNotes
    {
        get => recentNotes.Where(x => !_notesOrchestrator.OpenNotes.Contains(x)).Take(RecentNotesCapacity).ToList();
    }

    public int RecentNotesCapacity { get; set; }

    public NoteData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            HeaderHighlight = HeaderHighlight,
            FontFamily = FontFamily,
            FontSize = FontSize,
            RelativeFilePath = NotePath?.Relative,
            Editing = Editing,
            RecentNotes = recentNotes.Take(RecentNotesCapacity).Select(x => x.Relative).ToList(),
            RecentNotesCapacity = RecentNotesCapacity
        };

    public static bool FolderCanBeCreated(string name, string directory)
    {
        if (string.Equals(name, "widgets", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = Path.Combine(directory, name);
        return !Directory.Exists(path);
    }

    public static bool NoteCanBeCreated(string name, string directory)
    {
        var path = Path.Combine(directory, $"{name}.md");
        return !File.Exists(path);
    }

    public IReadOnlyList<ExplorerItem> GetExplorerItems()
        => DirectoryToTreeViewNode(_notesOrchestrator.FileSystemItems, _storageService.StoragePath);

    private List<ExplorerItem> DirectoryToTreeViewNode(IReadOnlyList<FileSystemItem> fileSystemItems, string currentPath)
    {
        var folders = fileSystemItems
            .Where(x => x.Type == FileType.Folder && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExplorerItem(_notesOrchestrator, x.Name, FileType.Folder, x.Path, DirectoryToTreeViewNode(fileSystemItems, x.Path.Absolute)));

        var notes = fileSystemItems
            .Where(x => x.Type == FileType.Note && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExplorerItem(_notesOrchestrator, x.Name, FileType.Note, x.Path, [])
            {
                IsEnabled = x.SelectedInWidgetId is null || x.SelectedInWidgetId == Id
            });

        return folders.Concat(notes).ToList();
    }

    public static NoteWidget Load(IStorageService storageService, INotesOrchestrator notesOrchestrator, string markdown)
    {
        var data = NoteData.FromMarkdown(markdown);
        return new(storageService, notesOrchestrator, data);
    }

    public async Task WriteAsync()
    {
        DebugLogger.Log($"Writing out {Type} widget to file system. ID: {Id}.");

        var data = GetData();

        var builder = new StringBuilder();
        data.ToMarkdown(builder);
        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), builder.ToString());
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }
}
