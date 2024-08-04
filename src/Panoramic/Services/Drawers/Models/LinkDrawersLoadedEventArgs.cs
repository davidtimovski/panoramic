using System;
using System.Collections.Generic;
using Panoramic.Data;

namespace Panoramic.Services.Drawers.Models;

public sealed class LinkDrawersLoadedEventArgs : EventArgs
{
    public required IReadOnlyCollection<LinkDrawerData> Drawers { get; init; }
}
