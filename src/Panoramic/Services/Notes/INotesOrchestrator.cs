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
    event EventHandler<NoteContentChangedEventArgs>? NoteContentChanged;
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
    Task WriteUnsavedChangesAsync();

    Task SaveNoteWidgetContentAsync(NoteWidget widget);

    /// <summary>
    /// Schedules a note save to disk.
    /// Will reset the auto-save timer if other note changes have been enqueued.
    /// </summary>
    void EnqueueNoteWrite(FileSystemItemPath path, string text);

    void ChangeNoteSelection(Guid widgetId, FileSystemItemPath? previousFilePath, FileSystemItemPath? newFilePath);
    void ChangeNoteContent(Guid widgetId, FileSystemItemPath path, string content);

    void CreateFolder(Guid widgetId, string directory, string name);
    void RenameFolder(FileSystemItemPath path, string newName);
    void DeleteFolder(FileSystemItemPath path);
    void CreateNote(Guid widgetId, string directory, string name);
    void RenameNote(FileSystemItemPath path, string newName);
    void DeleteNote(FileSystemItemPath path);
}
