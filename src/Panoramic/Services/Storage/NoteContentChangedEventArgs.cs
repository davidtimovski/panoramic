using System;

namespace Panoramic.Services.Storage;

public sealed class NoteContentChangedEventArgs(Guid widgetId, string path, string content) : EventArgs
{
    public Guid WidgetId { get; } = widgetId;
    public string Path { get; } = path;
    public string Content { get; } = content;
}
