using System;

namespace Panoramic.Services.Storage;

public sealed class WidgetUpdatedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
