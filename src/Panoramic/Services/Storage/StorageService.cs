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

public class StorageService : IStorageService
{
    private const string PanoramicDirectoryName = "Panoramic";

    /// <summary>
    /// Used to write changed sections widget data to disk.
    /// </summary>
    private readonly DispatcherQueueTimer _timer;

    /// <summary>
    /// Stores the widgets that have been changed and need to be written to disk.
    /// </summary>
    private readonly HashSet<Guid> _unsavedWidgets = [];

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
            var tasks = new List<Task>(_unsavedWidgets.Count);

            foreach (var id in _unsavedWidgets)
            {
                var widget = Widgets[id];
                tasks.Add(widget.WriteAsync());
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _unsavedWidgets.Clear();
            _timer.Stop();
        };
    }

    public event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    public event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;
    public event EventHandler<FileCreatedEventArgs>? FileCreated;
    public event EventHandler<EventArgs>? FileRenamed;
    public event EventHandler<FileDeletedEventArgs>? FileDeleted;
    public event EventHandler<NoteSelectedEventArgs>? NoteSelected;

    public string WidgetsFolderPath => Path.Combine(StoragePath, "widgets");
    public string StoragePath { get; private set; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private List<FileSystemItem> fileSystemItems = [];
    public IReadOnlyList<FileSystemItem> FileSystemItems
    {
        get => fileSystemItems;
        private set
        {
            fileSystemItems = [.. value];
        }
    }
    public Dictionary<Guid, IWidget> Widgets { get; } = [];

    public async Task ReadAsync()
    {
        LoadFileSystemItems();

        var widgetFilePaths = Directory.GetFiles(WidgetsFolderPath, "*.json");

        var tasks = widgetFilePaths.Select(ReadWidgetAsync);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task WriteAsync()
    {
        var widgetKvps = Widgets.Where(x => x.Value is not null).ToList();

        var editedNotes = new List<ExplorerItem>();
        var saveWidgetTasks = new List<Task>(widgetKvps.Count);

        // Save widgets
        foreach (var widgetKvp in widgetKvps)
        {
            saveWidgetTasks.Add(widgetKvp.Value.WriteAsync());
        }

        await Task.WhenAll(saveWidgetTasks).ConfigureAwait(false);

        await WriteNotesAsync().ConfigureAwait(false);
    }

    public async Task WriteNotesAsync()
    {
        //var editedNotes = new List<FileSystemItem>();
        //CollectEditedNotes(FileSystemItems, editedNotes);

        //// Save unsaved notes across all Note widgets.
        //// If two widgets have changes to the same note, use the last changed one.
        //var unsavedNotes = editedNotes.OrderByDescending(x => x.LastEdited).ToList();
        //var saveNoteTasks = new List<Task>(unsavedNotes.Count);
        //foreach (var note in unsavedNotes)
        //{
        //    saveNoteTasks.Add(File.WriteAllTextAsync(note.Path, note.Text));
        //}

        //await Task.WhenAll(saveNoteTasks).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void EnqueueWidgetWrite(Guid id)
    {
        _unsavedWidgets.Add(id);

        _timer.Stop();
        _timer.Start();
    }

    public void DeleteWidget(IWidget widget)
    {
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
        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, true);
        }

        Directory.Move(StoragePath, storagePath);

        StoragePath = storagePath;
    }

    public void CreateFolder(string directory, string name)
    {
        var path = Path.Combine(directory, name);
        Directory.CreateDirectory(path);

        var folder = new FileSystemItem
        {
            Name = name,
            Type = FileType.Folder,
            Path = new(path, StoragePath)
        };
        AddItem(fileSystemItems, folder, directory);

        FileCreated?.Invoke(this, new FileCreatedEventArgs(name, FileType.Folder, path));
    }

    public void RenameFolder(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, newName);
        Directory.Move(path, destinationPath);

        LoadFileSystemItems();

        FileRenamed?.Invoke(this, new EventArgs());
    }

    public void DeleteFolder(string path)
    {
        Directory.Delete(path, true);

        FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
    }

    public void SelectNote(Guid widgetId, string? previousFilePath, string? newFilePath)
    {
        UpdateSelectedNote(fileSystemItems, widgetId, previousFilePath, newFilePath);
        NoteSelected?.Invoke(this, new NoteSelectedEventArgs(widgetId, previousFilePath, newFilePath));
    }

    public void CreateNote(string directory, string name)
    {
        var path = Path.Combine(directory, $"{name}.md");
        File.Create(path).Dispose();

        var note = new FileSystemItem
        {
            Name = name,
            Type = FileType.File,
            Path = new(path, StoragePath)
        };
        AddItem(fileSystemItems, note, directory);

        FileCreated?.Invoke(this, new FileCreatedEventArgs(name, FileType.File, path));
    }

    public void RenameNote(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, $"{newName}.md");
        File.Move(path, destinationPath);

        LoadFileSystemItems();

        FileRenamed?.Invoke(this, new EventArgs());
    }

    public void DeleteNote(string path)
    {
        File.Delete(path);

        FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
    }

    private static void SetSelectedNote(List<FileSystemItem> items, string path, Guid widgetId)
    {
        foreach (var item in items)
        {
            if (item.Type == FileType.Folder)
            {
                SetSelectedNote(item.Children, path, widgetId);
            }
            else
            {
                if (item.Path.Equals(path))
                {
                    item.SelectedInWidgetId = widgetId;
                }
                else if (item.SelectedInWidgetId == widgetId)
                {
                    item.SelectedInWidgetId = null;
                }
            }
        }
    }

    private static bool AddItem(List<FileSystemItem> items, FileSystemItem item, string directory)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Type == FileType.File)
            {
                continue;
            }

            if (items[i].Path.Equals(item.Path.Parent))
            {
                items[i].Children.Add(item);
                return true;
            }

            for (var j = 0; j < items[i].Children.Count; j++)
            {
                if (AddItem(items[i].Children, item, directory))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void UpdateSelectedNote(List<FileSystemItem> items, Guid widgetId, string? previousFilePath, string? newFilePath)
    {
        foreach (var item in items)
        {
            if (item.Type == FileType.Folder)
            {
                UpdateSelectedNote(item.Children, widgetId, previousFilePath, newFilePath);
            }
            else
            {
                if (item.Path.Equals(newFilePath))
                {
                    item.SelectedInWidgetId = widgetId;
                }
                else if (previousFilePath is not null && item.Path.Equals(previousFilePath))
                {
                    item.SelectedInWidgetId = null;
                }
            }
        }
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
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), PanoramicDirectoryName);
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            return defaultPath;
        }

        return (string)storagePathValue;
    }

    private void LoadFileSystemItems()
    {
        var rootNode = DirectoryToTreeViewNode(StoragePath, StoragePath);
        fileSystemItems = [rootNode];
    }

    private FileSystemItem DirectoryToTreeViewNode(string currentPath, string storagePath)
    {
        var node = new FileSystemItem
        {
            Name = Path.GetFileName(currentPath),
            Type = FileType.Folder,
            Path = new(currentPath, StoragePath)
        };

        var subdirectories = Directory.GetDirectories(currentPath).Where(x => !Equals(x, WidgetsFolderPath)).OrderBy(x => x).ToList();
        node.Children = subdirectories.Select(x => DirectoryToTreeViewNode(x, storagePath)).ToList();

        var filePaths = Directory.GetFiles(currentPath, "*.md").OrderBy(x => Path.GetFileName(x)).ToList();
        node.Children.AddRange(filePaths.Select(x => new FileSystemItem { Name = Path.GetFileNameWithoutExtension(x), Type = FileType.File, Path = new(x, StoragePath) }));

        return node;
    }
}
