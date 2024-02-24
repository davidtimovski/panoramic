using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Windows.Storage;

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
            await WriteUnsavedChangesAsync();

            _unsavedWidgets.Clear();
            _unsavedNotes.Clear();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;
    public event EventHandler<EventArgs>? StoragePathChanged;
    public event EventHandler<NoteSelectionChangedEventArgs>? NoteSelectionChanged;
    public event EventHandler<NoteContentChangedEventArgs>? NoteContentChanged;
    public event EventHandler<FileCreatedEventArgs>? FileCreated;
    public event EventHandler<EventArgs>? FileRenamed;
    public event EventHandler<FileDeletedEventArgs>? FileDeleted;

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
        LoadFileSystemItemsRecursive(StoragePath, StoragePath);

        var widgetFilePaths = Directory.GetFiles(WidgetsFolderPath, "*.json");

        var tasks = widgetFilePaths.Select(ReadWidgetAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
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
            tasks.Add(File.WriteAllTextAsync(note.Key, note.Value));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id)
    {
        _unsavedWidgets.Add(id);

        _timer.Stop();
        _timer.Start();
    }

    /// <inheritdoc/>
    public void EnqueueNoteWrite(string path, string text)
    {
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

        WidgetRemoved?.Invoke(this, new WidgetRemovedEventArgs(widget.Id));
    }

    public async Task AddNewWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync();

        if (!Widgets.TryAdd(widget.Id, widget))
        {
            Widgets[widget.Id] = widget;
        }

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(widget.Id));
    }

    public async Task SaveWidgetAsync(IWidget widget)
    {
        await widget.WriteAsync();

        WidgetUpdated?.Invoke(this, new WidgetUpdatedEventArgs(widget.Id));
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
        if (previousFilePath is not null)
        {
            fileSystemItems[previousFilePath].SelectedInWidgetId = null;
        }

        if (newFilePath is not null)
        {
            fileSystemItems[newFilePath].SelectedInWidgetId = widgetId;
        }

        NoteSelectionChanged?.Invoke(this, new NoteSelectionChangedEventArgs(widgetId, previousFilePath, newFilePath));
    }

    public void ChangeNoteContent(Guid widgetId, string path, string content)
    {
        fileSystemItems[path].Content = content;
        NoteContentChanged?.Invoke(this, new NoteContentChangedEventArgs(widgetId, path, content));
    }

    public void CreateFolder(Guid widgetId, string directory, string name)
    {
        var path = Path.Combine(directory, name);
        Directory.CreateDirectory(path);

        var folder = new FileSystemItem(FileType.Folder)
        {
            Name = name,
            Path = new(path, StoragePath)
        };
        fileSystemItems.Add(folder.Path.Absolute, folder);

        FileCreated?.Invoke(this, new FileCreatedEventArgs(widgetId, name, FileType.Folder, path));
    }

    public void RenameFolder(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, newName);
        Directory.Move(path, destinationPath);

        LoadFileSystemItemsRecursive(StoragePath, StoragePath);

        FileRenamed?.Invoke(this, new EventArgs());
    }

    public void DeleteFolder(string path)
    {
        Directory.Delete(path, true);

        FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
    }

    public void CreateNote(Guid widgetId, string directory, string name)
    {
        var path = Path.Combine(directory, $"{name}.md");
        File.Create(path).Dispose();

        var note = new FileSystemItem(FileType.Note)
        {
            Name = name,
            Path = new(path, StoragePath),
            SelectedInWidgetId = widgetId
        };
        fileSystemItems.Add(note.Path.Absolute, note);

        FileCreated?.Invoke(this, new FileCreatedEventArgs(widgetId, name, FileType.Note, path));
    }

    public void RenameNote(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, $"{newName}.md");
        File.Move(path, destinationPath);

        LoadFileSystemItemsRecursive(StoragePath, StoragePath);

        FileRenamed?.Invoke(this, new EventArgs());
    }

    public void DeleteNote(string path)
    {
        File.Delete(path);

        FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
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

        var filePaths = Directory.GetFiles(currentPath, "*.md").OrderBy(x => Path.GetFileName(x)).ToList();
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
}
