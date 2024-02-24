using System;

namespace Panoramic.Services.Storage;

public sealed class FileDeletedEventArgs : EventArgs
{
    public required string Path { get; init; }
}
