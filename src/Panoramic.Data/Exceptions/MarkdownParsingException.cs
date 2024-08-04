namespace Panoramic.Data.Exceptions;

public class MarkdownParsingException(string relativeFilePath, IReadOnlyList<string> lines, int potentialErrorLine) : Exception
{
    public string RelativeFilePath { get; } = relativeFilePath;
    public IReadOnlyList<string> Lines { get; } = lines;
    public int PotentialErrorLine { get; } = potentialErrorLine;
}
