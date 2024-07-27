using System;
using System.IO;

namespace Panoramic.Services.Storage.Models;

public sealed class FileSystemItemPath(string absolutePath, string storagePath)
{
    public string Absolute { get; } = absolutePath;
    public string Relative { get; } = Path.GetRelativePath(storagePath, absolutePath);
    public string Parent { get; } = Path.GetDirectoryName(absolutePath)!;

    public bool IsSubPathOf(FileSystemItemPath path)
    {
        var relativePath = Path.GetRelativePath(storagePath, path.Absolute);
        return Relative.StartsWith(relativePath, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is FileSystemItemPath fileSystemItemPath)
        {
            return string.Equals(Absolute, fileSystemItemPath.Absolute, StringComparison.OrdinalIgnoreCase);
        }

        throw new ArgumentException($"Cannot compare {nameof(FileSystemItemPath)} to {obj.GetType()}", nameof(obj));
    }

    public bool Equals(string? stringPath)
        => stringPath is not null && string.Equals(Absolute, stringPath, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Absolute.GetHashCode();
}
