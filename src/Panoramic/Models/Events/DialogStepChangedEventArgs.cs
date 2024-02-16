using System;

namespace Panoramic.Models.Events;

public sealed class DialogStepChangedEventArgs(string dialogTitle) : EventArgs
{
    public string DialogTitle { get; } = dialogTitle;
}
