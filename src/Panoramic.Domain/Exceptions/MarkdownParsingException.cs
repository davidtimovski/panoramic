namespace Panoramic.Data.Exceptions;

public class MarkdownParsingException(IReadOnlyList<string> lines, int potentialErrorLine) : Exception
{
    public IReadOnlyList<string> Lines { get; } = lines;
    public int PotentialErrorLine { get; } = potentialErrorLine;
    public string? FileName { get; set; }
}
