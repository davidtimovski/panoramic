using System;

namespace Panoramic.Services.Storage.Models;

public sealed class WidgetUpdatedEventArgs : EventArgs
{
    public required Guid Id { get; init; }
}
