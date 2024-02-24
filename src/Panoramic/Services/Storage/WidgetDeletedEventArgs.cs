using System;

namespace Panoramic.Services.Storage;

public sealed class WidgetDeletedEventArgs : EventArgs
{
    public required Guid Id { get; init; }
}
