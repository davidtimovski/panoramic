using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Models.Domain;

namespace Panoramic.Services.Storage;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetDeletedEventArgs>? WidgetDeleted;
    event EventHandler<EventArgs>? StoragePathChanged;
    event EventHandler<NoteSelectionChangedEventArgs>? NoteSelectionChanged;
    event EventHandler<NoteContentChangedEventArgs>? NoteContentChanged;
    event EventHandler<FileCreatedEventArgs>? FileCreated;
    event EventHandler<FileDeletedEventArgs>? FileDeleted;
    event EventHandler<EventArgs>? ItemRenamed;

    string WidgetsFolderPath { get; }
    string StoragePath { get; }
    JsonSerializerOptions SerializerOptions { get; }
    IReadOnlyList<FileSystemItem> FileSystemItems { get; }
    Dictionary<Guid, IWidget> Widgets { get; }

    Task ReadAsync();
    Task WriteUnsavedChangesAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueWidgetWrite(Guid id);

    /// <summary>
    /// Schedules a note save to disk.
    /// Will reset the timer if other changes have been scheduled.
    /// </summary>
    void EnqueueNoteWrite(string path, string text);

    void DeleteWidget(IWidget widget);
    Task AddNewWidgetAsync(IWidget widget);
    Task SaveWidgetAsync(IWidget widget);

    void ChangeStoragePath(string storagePath);

    void ChangeNoteSelection(Guid widgetId, string? previousFilePath, string? newFilePath);
    void ChangeNoteContent(Guid widgetId, string path, string content);

    void CreateFolder(Guid widgetId, string directory, string name);
    void RenameFolder(string path, string newName);
    void DeleteFolder(string path);
    void CreateNote(Guid widgetId, string directory, string name);
    void RenameNote(string path, string newName);
    void DeleteNote(string path);
}
