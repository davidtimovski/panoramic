using System;

namespace Panoramic.Services.Storage.Models;

public sealed class WidgetDeletedEventArgs : EventArgs
{
    public required Guid Id { get; init; }
}
