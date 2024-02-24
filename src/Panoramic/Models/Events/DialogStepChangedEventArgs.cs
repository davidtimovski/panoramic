using System;

namespace Panoramic.Models.Events;

public sealed class DialogStepChangedEventArgs : EventArgs
{
    public required string DialogTitle { get; init; }
}
