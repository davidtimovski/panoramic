using System;

namespace Panoramic.Services.Notes;

public class NotesException : Exception
{
    public NotesException(Exception innerException) : this("An unexpected error occurred", innerException) { }
    public NotesException(string message, Exception innerException) : base(message, innerException) { }
}
