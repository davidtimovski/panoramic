using System;

namespace Panoramic.Models.Events;

public class DialogStepChangedEventArgs(string dialogTitle) : EventArgs
{
    public string DialogTitle { get; } = dialogTitle;
}
