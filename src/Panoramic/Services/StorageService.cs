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

namespace Panoramic.Services;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;

    string WidgetsFolderPath { get; }
    string StoragePath { get; }
    JsonSerializerOptions SerializerOptions { get; }
    IReadOnlyList<ExplorerItem> ExplorerItems { get; }
    Dictionary<Guid, IWidget> Widgets { get; }

    Task ReadAsync();
    Task WriteAsync();
    Task WriteNotesAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueWidgetWrite(Guid id);

    void DeleteWidget(IWidget widget);
    Task AddNewWidgetAsync(IWidget widget);
    Task SaveWidgetAsync(IWidget widget);

    void ChangeStoragePath(string storagePath);

    void CreateFolder(string directory, string name);
    void RenameFolder(string path, string newName);
    void DeleteFolder(string path);
    void CreateNote(string directory, string name);
    void RenameNote(string path, string newName);
    void DeleteNote(string path);
}

public class StorageService : IStorageService
{
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

    public string WidgetsFolderPath => Path.Combine(StoragePath, "widgets");
    public string StoragePath { get; private set; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<ExplorerItem> ExplorerItems { get; private set; }
    public Dictionary<Guid, IWidget> Widgets { get; } = [];

    public async Task ReadAsync()
    {
        ExplorerItems = GetExplorerItems();

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
        var editedNotes = new List<ExplorerItem>();
        CollectEditedNotes(ExplorerItems, editedNotes);

        // Save unsaved notes across all Note widgets.
        // If two widgets have changes to the same note, use the last changed one.
        var unsavedNotes = editedNotes.OrderByDescending(x => x.LastEdited).ToList();
        var saveNoteTasks = new List<Task>(unsavedNotes.Count);
        foreach (var note in unsavedNotes)
        {
            saveNoteTasks.Add(File.WriteAllTextAsync(note.Path, note.Text));
        }

        await Task.WhenAll(saveNoteTasks).ConfigureAwait(false);
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
    }

    public void RenameFolder(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, newName);
        Directory.Move(path, destinationPath);
    }

    public void DeleteFolder(string path) => Directory.Delete(path, true);

    public void CreateNote(string directory, string name)
    {
        var path = Path.Combine(directory, $"{name}.md");
        File.Create(path);
    }

    public void RenameNote(string path, string newName)
    {
        var directory = Path.GetDirectoryName(path)!;
        var destinationPath = Path.Combine(directory, $"{newName}.md");
        File.Move(path, destinationPath);
    }

    public void DeleteNote(string path) => File.Delete(path);

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
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Panoramic");
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            return defaultPath;
        }

        return (string)storagePathValue;
    }


    /// <summary>
    /// Retrieves directories and .md files from the storage path and maps them to a hierarchy of <see cref="ExplorerItem"/>.
    /// </summary>
    private List<ExplorerItem> GetExplorerItems()
    {
        var node = DirectoryToTreeViewNode(StoragePath, StoragePath);
        return node.Children.ToList();
    }

    private ExplorerItem DirectoryToTreeViewNode(string currentPath, string storagePath)
    {
        var node = new ExplorerItem
        {
            Name = Path.GetFileName(currentPath),
            IsExpanded = true,
            Type = FileType.Folder,
            Path = currentPath
        };

        var subdirectories = Directory.GetDirectories(currentPath).Where(x => !Equals(x, WidgetsFolderPath)).OrderBy(x => x).ToList();
        foreach (var subdirectory in subdirectories)
        {
            node.Children.Add(DirectoryToTreeViewNode(subdirectory, storagePath));
        }

        var filePaths = Directory.GetFiles(currentPath, "*.md").OrderBy(x => Path.GetFileName(x)).ToList();
        foreach (var filePath in filePaths)
        {
            node.Children.Add(new ExplorerItem { Name = Path.GetFileNameWithoutExtension(filePath), Type = FileType.File, Path = filePath });
        }

        return node;
    }

    private static void CollectEditedNotes(IReadOnlyList<ExplorerItem> items, List<ExplorerItem> result)
    {
        foreach (var item in items)
        {
            if (item.LastEdited.HasValue)
            {
                result.Add(item);
            }

            CollectEditedNotes(item.Children, result);
        }
    }
}

public class WidgetUpdatedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}

public class WidgetRemovedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
