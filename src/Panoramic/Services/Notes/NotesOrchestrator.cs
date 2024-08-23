using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.Note;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.Utils;

namespace Panoramic.Services.Notes;

/// <inheritdoc/>
public sealed class NotesOrchestrator : INotesOrchestrator
{
    private readonly IStorageService _storageService;

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Holds the notes that have been changed and need to be written to disk.
    /// </summary>
    private readonly Dictionary<string, string> _unsavedNotes = [];

    public NotesOrchestrator(IStorageService storageService)
    {
        _storageService = storageService;

        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(15);
        _timer.Tick += async (timer, _) =>
        {
            DebugLogger.Log($"Running auto-save for {_unsavedNotes.Count} notes..");

            await WriteUnsavedChangesAsync();
        };
    }

    public event EventHandler<NoteSelectionChangedEventArgs>? NoteSelectionChanged;
    public event EventHandler<NoteContentChangedEventArgs>? NoteContentChanged;
    public event EventHandler<FileCreatedEventArgs>? FileCreated;
    public event EventHandler<FileDeletedEventArgs>? FileDeleted;
    public event EventHandler<EventArgs>? ItemRenamed;

    private Dictionary<string, FileSystemItem> fileSystemItems = [];
    public IReadOnlyList<FileSystemItem> FileSystemItems
    {
        get => fileSystemItems.Values.ToList();
        private set
        {
            fileSystemItems = value.ToDictionary(x => x.Path.Absolute, x => x);
        }
    }

    /// <inheritdoc/>
    public HashSet<FileSystemItemPath> OpenNotes { get; } = [];

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IWidget>> ReadWidgetsAsync()
    {
        try
        {
            LoadFileSystemItems();

            var noteWidgetFilePaths = Directory.GetFiles(_storageService.WidgetsFolderPath, "note-*.md");

            var readNoteWidgetTasks = noteWidgetFilePaths.Select(ReadNoteWidgetAsync);

            var noteWidgets = await Task.WhenAll(readNoteWidgetTasks).ConfigureAwait(false);

            SetSelectedNotes(noteWidgets.OfType<NoteWidget>().ToList());

            return noteWidgets;
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    /// <inheritdoc/>
    public async Task SaveNoteWidgetAsync(NoteWidget widget)
    {
        try
        {
            if (widget.NotePath is not null && _unsavedNotes.TryGetValue(widget.NotePath.Absolute, out string? content))
            {
                await WriteNoteAsync(widget.NotePath.Absolute, content);
            }

            await _storageService.SaveWidgetAsync(widget);
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public async Task WriteUnsavedChangesAsync()
    {
        try
        {
            _timer.Stop();

            var tasks = new List<Task>(_unsavedNotes.Count);

            foreach (var note in _unsavedNotes)
            {
                tasks.Add(WriteNoteAsync(note.Key, note.Value));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _unsavedNotes.Clear();
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    /// <inheritdoc/>
    public void EnqueueNoteWrite(FileSystemItemPath path, string text)
    {
        try
        {
            DebugLogger.Log($"Enqueuing note content write: {path}");

            if (!_unsavedNotes.TryAdd(path.Absolute, text))
            {
                _unsavedNotes[path.Absolute] = text;
            }

            _timer.Stop();
            _timer.Start();
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void ChangeNoteSelection(Guid widgetId, FileSystemItemPath? previousFilePath, FileSystemItemPath? newFilePath)
    {
        try
        {
            if (previousFilePath is not null && fileSystemItems.TryGetValue(previousFilePath.Absolute, out FileSystemItem? value))
            {
                value.SelectedInWidgetId = null;
                OpenNotes.Remove(previousFilePath);
            }

            if (newFilePath is not null)
            {
                fileSystemItems[newFilePath.Absolute].SelectedInWidgetId = widgetId;
                OpenNotes.Add(newFilePath);
            }

            NoteSelectionChanged?.Invoke(this, new NoteSelectionChangedEventArgs
            {
                WidgetId = widgetId,
                PreviousFilePath = previousFilePath,
                NewFilePath = newFilePath
            });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void ChangeNoteContent(Guid widgetId, FileSystemItemPath path, string content)
    {
        try
        {
            if (!fileSystemItems.ContainsKey(path.Absolute))
            {
                return;
            }

            NoteContentChanged?.Invoke(this, new NoteContentChangedEventArgs
            {
                WidgetId = widgetId,
                Path = path,
                Content = content
            });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void CreateFolder(Guid widgetId, string directory, string name)
    {
        try
        {
            name = name.Trim();

            var path = Path.Combine(directory, name);
            Directory.CreateDirectory(path);

            var folder = new FileSystemItem
            {
                Name = name,
                Type = FileType.Folder,
                Path = new(path, _storageService.StoragePath)
            };
            fileSystemItems.Add(folder.Path.Absolute, folder);

            FileCreated?.Invoke(this, new FileCreatedEventArgs
            {
                WidgetId = widgetId,
                Name = name,
                Type = FileType.Folder,
                Path = new(path, _storageService.StoragePath)
            });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void RenameFolder(FileSystemItemPath path, string newName)
    {
        try
        {
            newName = newName.Trim();

            var destinationPath = Path.Combine(path.Parent, newName);
            Directory.Move(path.Absolute, destinationPath);

            fileSystemItems.Clear();
            LoadFileSystemItems();

            var noteWidgets = _storageService.Widgets.Values.OfType<NoteWidget>().ToList();
            SetSelectedNotes(noteWidgets);

            ItemRenamed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void DeleteFolder(FileSystemItemPath path)
    {
        try
        {
            Directory.Delete(path.Absolute, true);

            fileSystemItems.Clear();
            LoadFileSystemItems();

            var noteWidgets = _storageService.Widgets.Values.OfType<NoteWidget>().ToList();
            SetSelectedNotes(noteWidgets);

            FileDeleted?.Invoke(this, new FileDeletedEventArgs { Path = path });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void CreateNote(Guid widgetId, string directory, string name)
    {
        try
        {
            name = name.Trim();

            var path = Path.Combine(directory, $"{name}.md");
            File.Create(path).Dispose();

            var note = new FileSystemItem
            {
                Name = name,
                Type = FileType.Note,
                Path = new(path, _storageService.StoragePath),
                SelectedInWidgetId = widgetId
            };
            fileSystemItems.Add(note.Path.Absolute, note);

            FileCreated?.Invoke(this, new FileCreatedEventArgs
            {
                WidgetId = widgetId,
                Name = name,
                Type = FileType.Note,
                Path = new(path, _storageService.StoragePath)
            });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void RenameNote(FileSystemItemPath path, string newName)
    {
        try
        {
            newName = newName.Trim();

            var newPath = Path.Combine(path.Parent, $"{newName}.md");

            if (_unsavedNotes.TryGetValue(path.Absolute, out string? content))
            {
                _unsavedNotes.Remove(path.Absolute);
                _unsavedNotes.Add(newPath, content);
            }

            File.Move(path.Absolute, newPath);

            var item = fileSystemItems[path.Absolute];
            item.Name = newName;
            item.Path = new(newPath, _storageService.StoragePath);
            fileSystemItems.Remove(path.Absolute);
            fileSystemItems.Add(newPath, item);

            ItemRenamed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    public void DeleteNote(FileSystemItemPath path)
    {
        try
        {
            _unsavedNotes.Remove(path.Absolute);

            File.Delete(path.Absolute);

            fileSystemItems.Remove(path.Absolute);

            FileDeleted?.Invoke(this, new FileDeletedEventArgs { Path = path });
        }
        catch (Exception ex)
        {
            throw new NotesException(ex);
        }
    }

    private async Task<IWidget> ReadNoteWidgetAsync(string widgetFilePath)
    {
        var relativeFilePath = Path.GetRelativePath(_storageService.StoragePath, widgetFilePath);
        var markdown = await File.ReadAllTextAsync(widgetFilePath);

        return NoteWidget.Load(_storageService, this, relativeFilePath, markdown);
    }

    private void LoadFileSystemItems() => LoadFileSystemItemsRecursive(_storageService.StoragePath, _storageService.StoragePath);

    private void SetSelectedNotes(List<NoteWidget> noteWidgets)
    {
        var selectedNotesLookup = noteWidgets.Where(x => x.NotePath is not null).ToDictionary(x => x.NotePath!, x => x.Id);

        foreach (var selectedNote in selectedNotesLookup)
        {
            fileSystemItems[selectedNote.Key.Absolute].SelectedInWidgetId = selectedNote.Value;

            OpenNotes.Add(selectedNote.Key);
        }
    }

    private void LoadFileSystemItemsRecursive(string currentPath, string storagePath)
    {
        var subdirectories = Directory.GetDirectories(currentPath)
            .Where(x => !Equals(x, _storageService.SystemFolderPath) && !Equals(x, _storageService.WidgetsFolderPath))
            .OrderBy(x => x).ToList();

        foreach (var subdirectory in subdirectories)
        {
            var item = new FileSystemItem
            {
                Name = Path.GetFileName(subdirectory),
                Type = FileType.Folder,
                Path = new(subdirectory, _storageService.StoragePath)
            };
            fileSystemItems.Add(item.Path.Absolute, item);

            LoadFileSystemItemsRecursive(subdirectory, storagePath);
        }

        var filePaths = Directory.GetFiles(currentPath, "*.md").OrderBy(Path.GetFileName).ToList();
        foreach (var filePath in filePaths)
        {
            var note = new FileSystemItem
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Type = FileType.Note,
                Path = new(filePath, _storageService.StoragePath)
            };

            fileSystemItems.Add(note.Path.Absolute, note);
        }
    }

    private static async Task WriteNoteAsync(string path, string content)
    {
        DebugLogger.Log($"Writing note content: {path}");

        await File.WriteAllTextAsync(path, content);
    }
}
