using System;

namespace Panoramic.Services.Storage;

public class WidgetRemovedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
