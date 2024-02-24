using System;

namespace Panoramic.Models.Events;

public sealed class ValidationEventArgs : EventArgs
{
    public required bool Valid { get; init; }
}
