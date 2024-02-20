using System;
using System.Collections.Generic;
using Panoramic.Services.Storage;

namespace Panoramic.Services;

public sealed class FileSystemItem
{
    public FileSystemItem(FileType type)
    {
        Type = type;
        Content = type == FileType.Note ? string.Empty : null;
    }

    public required string Name { get; set; }
    public FileType Type { get; }
    public required FileSystemItemPath Path { get; set; }
    public List<FileSystemItem> Children = [];
    public string? Content { get; set; }
    public Guid? SelectedInWidgetId { get; set; }
}
