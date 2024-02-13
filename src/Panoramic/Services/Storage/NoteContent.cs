using System;

namespace Panoramic.Services;

// TODO
public sealed class NoteContent
{
    private string text = string.Empty;
    public required string Text
    {
        get => text;
        set
        {
            text = value;
            LastEdited = DateTime.Now;
        }
    }
    public DateTime? LastEdited { get; private set; }
}
