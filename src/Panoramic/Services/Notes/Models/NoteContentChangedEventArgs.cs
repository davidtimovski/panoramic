using System;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes.Models;

public sealed class NoteContentChangedEventArgs : EventArgs
{
    public required Guid WidgetId { get; init; }
    public required FileSystemItemPath Path { get; init; }
    public required string Content { get; init; }
}
