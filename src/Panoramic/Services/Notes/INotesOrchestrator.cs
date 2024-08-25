using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.Note;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes;

/// <summary>
/// Handles orchestration for functionality that stretches across multiple Note widgets.
/// </summary>
public interface INotesOrchestrator
{
    event EventHandler<NoteSelectionChangedEventArgs>? NoteSelectionChanged;
    event EventHandler<FileCreatedEventArgs>? FileCreated;
    event EventHandler<FileDeletedEventArgs>? FileDeleted;
    event EventHandler<EventArgs>? ItemRenamed;

    IReadOnlyList<FileSystemItem> FileSystemItems { get; }

    /// <summary>
    /// Notes that are currently open in the UI.
    /// </summary>
    HashSet<FileSystemItemPath> OpenNotes { get; }

    /// <summary>
    /// Reads note widgets from the file system.
    /// </summary>
    Task<IReadOnlyList<IWidget>> ReadWidgetsAsync();

    /// <summary>
    /// Saves the note content before calling the default widget save.
    /// </summary>
    Task SaveNoteWidgetAsync(NoteWidget widget);

    Task WriteUnsavedChangesAsync();

    /// <summary>
    /// Reads the content from memory or if it's not present then from the file system.
    /// </summary>
    string GetContent(FileSystemItemPath path);

    /// <summary>
    /// Sets the note content in memory and schedules a note save to disk.
    /// Will reset the auto-save timer if other note changes have been enqueued.
    /// </summary>
    void SetContent(FileSystemItemPath path, string content);

    void ChangeNoteSelection(Guid widgetId, FileSystemItemPath? previousFilePath, FileSystemItemPath? newFilePath);

    void CreateFolder(Guid widgetId, string directory, string name);
    void RenameFolder(FileSystemItemPath path, string newName);
    void DeleteFolder(FileSystemItemPath path);
    void CreateNote(Guid widgetId, string directory, string name);
    void RenameNote(FileSystemItemPath path, string newName);
    void DeleteNote(FileSystemItemPath path);
}
