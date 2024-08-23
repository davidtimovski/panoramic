using System;

namespace Panoramic.Services.Storage;

public class StorageException : Exception
{
    public StorageException(Exception innerException) : this("An unexpected error occurred", innerException) { }
    public StorageException(string message, Exception innerException) : base(message, innerException) { }
}
