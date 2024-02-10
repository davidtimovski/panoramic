using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Models.Domain;

namespace Panoramic.Services.Storage;

public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetRemovedEventArgs>? WidgetRemoved;
    event EventHandler<FileCreatedEventArgs>? FileCreated;
    event EventHandler<EventArgs>? FileRenamed;
    event EventHandler<FileDeletedEventArgs>? FileDeleted;
    event EventHandler<NoteSelectedEventArgs>? NoteSelected;

    string WidgetsFolderPath { get; }
    string StoragePath { get; }
    JsonSerializerOptions SerializerOptions { get; }
    IReadOnlyList<FileSystemItem> FileSystemItems { get; }
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
    void SelectNote(Guid widgetId, string? previousFilePath, string? newFilePath);
    void CreateNote(string directory, string name);
    void RenameNote(string path, string newName);
    void DeleteNote(string path);
}
