using System;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Notes.Models;

public sealed class FileDeletedEventArgs : EventArgs
{
    public required FileSystemItemPath Path { get; init; }
}
