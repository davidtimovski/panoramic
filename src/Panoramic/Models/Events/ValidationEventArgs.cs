using System;

namespace Panoramic.Models.Events;

public sealed class ValidationEventArgs(bool valid) : EventArgs
{
    public bool Valid { get; } = valid;
}
