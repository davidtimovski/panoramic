using System;

namespace Panoramic.Services.Storage;

public sealed class FileDeletedEventArgs(string path) : EventArgs
{
    public string Path { get; } = path;
}
