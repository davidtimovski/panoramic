using System;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes.Models;

public sealed class NoteSelectionChangedEventArgs : EventArgs
{
    public required Guid WidgetId { get; init; }
    public required FileSystemItemPath? PreviousFilePath { get; init; }
    public required FileSystemItemPath? NewFilePath { get; init; }
}
