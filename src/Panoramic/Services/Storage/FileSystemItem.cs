using System;

namespace Panoramic.Services.Storage;

public sealed class FileSystemItem
{
    public required string Name { get; set; }
    public required FileType Type { get; init; }
    public required FileSystemItemPath Path { get; set; }
    public Guid? SelectedInWidgetId { get; set; }
}
