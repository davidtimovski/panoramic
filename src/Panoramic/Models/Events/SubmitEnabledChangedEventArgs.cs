using System;

namespace Panoramic.Models.Events;

public class SubmitEnabledChangedEventArgs : EventArgs
{
    public SubmitEnabledChangedEventArgs(bool enabled)
    {
        Enabled = enabled;
    }

    public bool Enabled { get; }
}
