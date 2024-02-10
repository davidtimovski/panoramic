using System;

namespace Panoramic.Services.Storage;

public class FileCreatedEventArgs(string name, FileType type, string path) : EventArgs
{
    public string Name { get; } = name;
    public FileType Type { get; } = type;
    public string Path { get; } = path;
}
