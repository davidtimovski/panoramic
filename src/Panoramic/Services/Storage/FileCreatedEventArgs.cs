using System;

namespace Panoramic.Services.Storage;

public sealed class FileCreatedEventArgs(Guid widgetId, string name, FileType type, string path) : EventArgs
{
    public Guid WidgetId { get; } = widgetId;
    public string Name { get; } = name;
    public FileType Type { get; } = type;
    public string Path { get; } = path;
}
