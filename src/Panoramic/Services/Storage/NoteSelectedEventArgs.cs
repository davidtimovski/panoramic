using System;

namespace Panoramic.Services.Storage;

public sealed class NoteSelectedEventArgs(Guid widgetId, string? previousFilePath, string? newFilePath) : EventArgs
{
    public Guid WidgetId { get; } = widgetId;
    public string? PreviousFilePath { get; } = previousFilePath;
    public string? NewFilePath { get; } = newFilePath;
}
