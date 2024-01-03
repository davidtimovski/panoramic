using System;

namespace Panoramic.Models.Events;

public class ValidationEventArgs(bool valid) : EventArgs
{
    public bool Valid { get; } = valid;
}
