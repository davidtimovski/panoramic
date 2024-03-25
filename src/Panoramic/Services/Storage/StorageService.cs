using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Dispatching;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Utils;

namespace Panoramic.Services.Storage;

public sealed class StorageService : IStorageService
{
    private const string DefaultDirectoryName = "Panoramic";

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Holds the widgets that have been changed and need to be written to disk.
    /// </summary>
    private readonly HashSet<Guid> _unsavedWidgets = [];

    /// <summary>
    /// Holds the notes that have been changed and need to be written to disk.
    /// </summary>
    private readonly Dictionary<string, string> _unsavedNotes = [];

    public StorageService()
    {
        StoragePath = InitializeStoragePath();

        if (!Directory.Exists(WidgetsFolderPath))
        {
            Directory.CreateDirectory(WidgetsFolderPath);
        }

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            DebugLogger.Log("Running auto save..");

            await WriteUnsavedChangesAsync();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetDeletedEventArgs>? WidgetDeleted;
    public event EventHandler<EventArgs>? StoragePathChanged;
    public event EventHandler<NoteSelectionChangedEventArgs>? NoteSelectionChanged;
    public event EventHandler<NoteContentChangedEventArgs>? NoteContentChanged;
    public event EventHandler<FileCreatedEventArgs>? FileCreated;
    public event EventHandler<FileDeletedEventArgs>? FileDeleted;
    public event EventHandler<EventArgs>? ItemRenamed;

    public string WidgetsFolderPath => Path.Combine(StoragePath, "widgets");
    public string StoragePath { get; private set; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private Dictionary<string, FileSystemItem> fileSystemItems = [];
    public IReadOnlyList<FileSystemItem> FileSystemItems
    {
        get => fileSystemItems.Values.ToList();
        private set
        {
            fileSystemItems = value.ToDictionary(x => x.Path.Absolute, x => x);
        }
    }

    public Dictionary<Guid, IWidget> Widgets { get; } = [];

    public async Task ReadAsync()
    {
        LoadFileSystemItems();

        var widgetFilePaths = Directory.GetFiles(WidgetsFolderPath, "*.json");

        var tasks = widgetFilePaths.Select(ReadWidgetAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        SetSelectedNotes();
    }

    public async Task WriteUnsavedChangesAsync()
    {
        _timer.Stop();

        var tasks = new List<Task>(_unsavedWidgets.Count + _unsavedNotes.Count);

        foreach (var id in _unsavedWidgets)
        {
            tasks.Add(Widgets[id].WriteAsync());
        }

        foreach (var note in _unsavedNotes)
        {
            tasks.Add(WriteNoteAsync(note.Key, note.Value));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _unsavedWidgets.Clear();
        _unsavedNotes.Clear();
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id)
    {
        DebugLogger.Log($"Enqueuing widget write: {id}");

        _unsavedWidgets.Add(id);

        _timer.Stop();
        _timer.Start();
    }

    /// <inheritdoc/>
    public void EnqueueNoteWrite(string path, string text)
    {
        DebugLogger.Log($"Enqueuing note content write: {path}");

        if (!_unsavedNotes.TryAdd(path, text))
        {
            _unsavedNotes[path] = text;
        }

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(IWidget widget)
    {
        if (_unsavedWidgets.Contains(widget.Id))
        {
            _unsavedWidgets.Remove(widget.Id);
        }

        widget.Delete();

        Widgets.Remove(widget.Id);

        WidgetDeleted?.Invoke(this, new WidgetDeletedEventArgs { Id = widget.Id });
    }

    public async Task AddNewWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync();

        if (!Widgets.TryAdd(widget.Id, widget))
        {
            Widgets[widget.Id] = widget;
        }

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
    }

    public async Task SaveWidgetAsync(IWidget widget)
    {
        // Write out any unsaved note content changes
        if (widget is NoteWidget noteWidget
            && noteWidget.NotePath is not null
            && _unsavedNotes.TryGetValue(noteWidget.NotePath.Absolute, out string? content))
        {
            await WriteNoteAsync(noteWidget.NotePath.Absolute, content);
        }

        if (_unsavedWidgets.Contains(widget.Id))
        {
            _unsavedWidgets.Remove(widget.Id);
        }

        await widget.WriteAsync();

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs { Id = widget.Id });
    }

    public void ChangeStoragePath(string storagePath)
    {
        if (string.Equals(StoragePath, storagePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, true);
        }

        Directory.Move(StoragePath, storagePath);

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[nameof(StoragePath)] = storagePath;

        StoragePath = storagePath;

        StoragePathChanged?.Invoke(this, new EventArgs());
    }

    public void ChangeNoteSelection(Guid widgetId, string? previousFilePath, string? newFilePath)
    {
        if (previousFilePath is not null && fileSystemItems.TryGetValue(previousFilePath, out FileSystemItem? value))
        {
            value.SelectedInWidgetId = null;
        }

        if (newFilePath is not null)
        {
            fileSystemItems[newFilePath].SelectedInWidgetId = widgetId;
        }

        NoteSelectionChanged?.Invoke(this, new NoteSelectionChangedEventArgs
        {
            WidgetId = widgetId,
            PreviousFilePath = previousFilePath,
            NewFilePath = newFilePath
        });
    }

    public void ChangeNoteContent(Guid widgetId, string path, string content)
    {
        if (fileSystemItems.TryGetValue(path, out FileSystemItem? value))
        {
            value.Content = content;

            NoteContentChanged?.Invoke(this, new NoteContentChangedEventArgs
            {
                WidgetId = widgetId,
                Path = path,
                Content = content
            });
        }
    }

    public void CreateFolder(Guid widgetId, string directory, string name)
    {
        name = name.Trim();

        var path = Path.Combine(directory, name);
        Directory.CreateDirectory(path);

        var folder = new FileSystemItem(FileType.Folder)
        {
            Name = name,
            Path = new(path, StoragePath)
        };
        fileSystemItems.Add(folder.Path.Absolute, folder);

        FileCreated?.Invoke(this, new FileCreatedEventArgs
        {
            WidgetId = widgetId,
            Name = name,
            Type = FileType.Folder,
            Path = new(path, StoragePath)
        });
    }

    public void RenameFolder(string path, string newName)
    {
        newName = newName.Trim();

        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, newName);
        Directory.Move(path, destinationPath);

        fileSystemItems.Clear();
        LoadFileSystemItems();
        SetSelectedNotes();

        ItemRenamed?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteFolder(string path)
    {
        Directory.Delete(path, true);

        fileSystemItems.Clear();
        LoadFileSystemItems();
        SetSelectedNotes();

        FileDeleted?.Invoke(this, new FileDeletedEventArgs { Path = path });
    }

    public void CreateNote(Guid widgetId, string directory, string name)
    {
        name = name.Trim();

        var path = Path.Combine(directory, $"{name}.md");
        File.Create(path).Dispose();

        var note = new FileSystemItem(FileType.Note)
        {
            Name = name,
            Path = new(path, StoragePath),
            SelectedInWidgetId = widgetId
        };
        fileSystemItems.Add(note.Path.Absolute, note);

        FileCreated?.Invoke(this, new FileCreatedEventArgs
        {
            WidgetId = widgetId,
            Name = name,
            Type = FileType.Note,
            Path = new(path, StoragePath)
        });
    }

    public void RenameNote(string path, string newName)
    {
        newName = newName.Trim();

        var directory = Path.GetDirectoryName(path)!;
        var newPath = Path.Combine(directory, $"{newName}.md");

        if (_unsavedNotes.TryGetValue(path, out string? content))
        {
            _unsavedNotes.Remove(path);
            _unsavedNotes.Add(newPath, content);
        }

        File.Move(path, newPath);

        var item = fileSystemItems[path];
        item.Name = newName;
        item.Path = new(newPath, StoragePath);
        fileSystemItems.Remove(path);
        fileSystemItems.Add(newPath, item);

        ItemRenamed?.Invoke(this, new EventArgs());
    }

    public void DeleteNote(string path)
    {
        _unsavedNotes.Remove(path);

        File.Delete(path);

        fileSystemItems.Remove(path);

        FileDeleted?.Invoke(this, new FileDeletedEventArgs { Path = path });
    }

    private async Task ReadWidgetAsync(string widgetFilePath)
    {
        var json = await File.ReadAllTextAsync(widgetFilePath);

        using var jsonDoc = JsonDocument.Parse(json);
        var typeProperty = jsonDoc.RootElement.GetProperty("type");
        var type = Enum.Parse<WidgetType>(typeProperty.GetString()!);

        switch (type)
        {
            case WidgetType.Note:
                var noteWidget = await NoteWidget.LoadAsync(this, json);
                Widgets.Add(noteWidget.Id, noteWidget);
                break;
            case WidgetType.LinkCollection:
                var linkCollectionWidget = LinkCollectionWidget.Load(this, json);
                Widgets.Add(linkCollectionWidget.Id, linkCollectionWidget);
                break;
            case WidgetType.RecentLinks:
                var recentLinksWidget = RecentLinksWidget.Load(this, json);
                Widgets.Add(recentLinksWidget.Id, recentLinksWidget);
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }
    }

    private string InitializeStoragePath()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        object? storagePathValue = localSettings.Values[nameof(StoragePath)];

        if (storagePathValue is null)
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultDirectoryName);
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            return defaultPath;
        }

        return (string)storagePathValue;
    }

    private void LoadFileSystemItems() => LoadFileSystemItemsRecursive(StoragePath, StoragePath);

    private void LoadFileSystemItemsRecursive(string currentPath, string storagePath)
    {
        var subdirectories = Directory.GetDirectories(currentPath).Where(x => !Equals(x, WidgetsFolderPath)).OrderBy(x => x).ToList();
        foreach (var subdirectory in subdirectories)
        {
            var item = new FileSystemItem(FileType.Folder)
            {
                Name = Path.GetFileName(subdirectory),
                Path = new(subdirectory, StoragePath)
            };
            fileSystemItems.Add(item.Path.Absolute, item);

            LoadFileSystemItemsRecursive(subdirectory, storagePath);
        }

        var filePaths = Directory.GetFiles(currentPath, "*.md").OrderBy(Path.GetFileName).ToList();
        foreach (var filePath in filePaths)
        {
            var note = new FileSystemItem(FileType.Note)
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Path = new(filePath, StoragePath)
            };

            fileSystemItems.Add(note.Path.Absolute, note);
        }
    }

    private void SetSelectedNotes()
    {
        var selectedNotesLookup = Widgets
            .Where(x => x.Value.Type == WidgetType.Note)
            .Select(x => x.Value).OfType<NoteWidget>()
            .Where(x => x.NotePath is not null)
            .ToDictionary(x => x.NotePath!.Absolute, x => x.Id);

        foreach (var selectedNote in  selectedNotesLookup)
        {
            fileSystemItems[selectedNote.Key].SelectedInWidgetId = selectedNote.Value;
        }
    }

    private static async Task WriteNoteAsync(string path, string content)
    {
        DebugLogger.Log($"Writing note content: {path}");

        await File.WriteAllTextAsync(path, content);
    }
}
