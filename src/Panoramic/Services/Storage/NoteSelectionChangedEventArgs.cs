using System;

namespace Panoramic.Services.Storage;

public sealed class NoteSelectionChangedEventArgs : EventArgs
{
    public required Guid WidgetId { get; init; }
    public required string? PreviousFilePath { get; init; }
    public required string? NewFilePath { get; init; }
}
