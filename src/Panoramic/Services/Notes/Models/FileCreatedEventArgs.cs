using System;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes.Models;

public sealed class FileCreatedEventArgs : EventArgs
{
    public required Guid WidgetId { get; init; }
    public required string Name { get; init; }
    public required FileType Type { get; init; }
    public required FileSystemItemPath Path { get; init; }
}
