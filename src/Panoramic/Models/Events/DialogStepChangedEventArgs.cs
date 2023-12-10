using System;

namespace Panoramic.Models.Events;

public class DialogStepChangedEventArgs : EventArgs
{
    public DialogStepChangedEventArgs(string dialogTitle)
    {
        DialogTitle = dialogTitle;
    }

    public string DialogTitle { get; }
}
