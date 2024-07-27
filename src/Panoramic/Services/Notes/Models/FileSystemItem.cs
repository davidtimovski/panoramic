using System;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes.Models;

public sealed class FileSystemItem
{
    public required string Name { get; set; }
    public required FileType Type { get; init; }
    public required FileSystemItemPath Path { get; set; }
    public Guid? SelectedInWidgetId { get; set; }
}
