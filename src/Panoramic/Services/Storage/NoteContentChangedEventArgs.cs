using System;

namespace Panoramic.Services.Storage;

public sealed class NoteContentChangedEventArgs : EventArgs
{
    public required Guid WidgetId { get; init; }
    public required string Path { get; init; }
    public required string Content { get; init; }
}
