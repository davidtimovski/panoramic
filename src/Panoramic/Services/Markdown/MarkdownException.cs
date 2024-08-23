using System;

namespace Panoramic.Services.Markdown;

public class MarkdownException : Exception
{
    public MarkdownException(Exception innerException) : this("An unexpected error occurred", innerException) { }
    public MarkdownException(string message, Exception innerException) : base(message, innerException) { }
}
