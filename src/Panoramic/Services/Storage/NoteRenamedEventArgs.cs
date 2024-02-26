using System;

namespace Panoramic.Services.Storage;

public sealed class NoteRenamedEventArgs : EventArgs
{
    public required string Name { get; init; }
    public required string OldPath { get; init; }
    public required string NewPath { get; init; }
}
