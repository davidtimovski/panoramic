using System;

namespace Panoramic.Services.Storage;

public sealed class WidgetRemovedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
