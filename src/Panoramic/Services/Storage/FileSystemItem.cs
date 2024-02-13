using System;
using System.Collections.Generic;
using Panoramic.Services.Storage;

namespace Panoramic.Services;

public sealed class FileSystemItem
{
    public required string Name { get; set; }
    public required FileType Type { get; init; }
    public required FileSystemItemPath Path { get; set; }
    public List<FileSystemItem> Children = [];
    public Guid? SelectedInWidgetId { get; set; }
}
