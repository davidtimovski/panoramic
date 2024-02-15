using System;

namespace Panoramic.Services.Storage;

public class FileDeletedEventArgs(string path) : EventArgs
{
    public string Path { get; } = path;
}
