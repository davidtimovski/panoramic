using System;

namespace Panoramic.Services.Storage;

public class WidgetUpdatedEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
