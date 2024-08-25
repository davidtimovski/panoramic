using System;
using Panoramic.Models.Domain;

namespace Panoramic.Services.Storage.Models;

public sealed class WidgetDeletedEventArgs : EventArgs
{
    public required IWidget Widget { get; init; }
}
