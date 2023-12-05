using System;

namespace Panoramic.Models.Events;

public class ValidationEventArgs : EventArgs
{
    public ValidationEventArgs(bool valid)
    {
        Valid = valid;
    }

    public bool Valid { get; }
}
