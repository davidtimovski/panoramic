using System.Text;
using Panoramic.Data.Exceptions;

namespace Panoramic.Data.Widgets;

public sealed class NoteData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public HighlightColor HeaderHighlight { get; init; }
    public string FontFamily { get; init; } = "Default";
    public double FontSize { get; init; } = 15;
    public string? RelativeFilePath { get; init; }

    public static NoteData FromMarkdown(string markdown)
    {
        var lineIndex = 6;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
            // Metadata
            var idRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var id = Guid.ParseExact(idRowValues[1], "N");
            lineIndex++;

            var areaRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var area = new Area(areaRowValues[1]);
            lineIndex++;

            var headerHighlightRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var headerHighlight = Enum.Parse<HighlightColor>(headerHighlightRowValues[1]);
            lineIndex++;

            var fontFamily = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];
            lineIndex++;

            var fontSizeRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var fontSize = double.Parse(fontSizeRowValues[1]);
            lineIndex++;

            var relativeFilePathRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var relativeFilePath = relativeFilePathRowValues.Length == 1 ? null : relativeFilePathRowValues[1];

            return new NoteData
            {
                Id = id,
                Area = area,
                HeaderHighlight = headerHighlight,
                FontFamily = fontFamily,
                FontSize = fontSize,
                RelativeFilePath = relativeFilePath
            };
        }
        catch
        {
            throw new MarkdownParsingException(lines, lineIndex);
        }
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# Note");
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();
        builder.AppendLine("| Key | Value |");
        builder.AppendLine("| - | - |");
        builder.AppendLine($"| {nameof(Id)} | {Id:N} |");
        builder.AppendLine($"| {nameof(Area)} | {Area} |");
        builder.AppendLine($"| {nameof(HeaderHighlight)} | {HeaderHighlight} |");
        builder.AppendLine($"| {nameof(FontFamily)} | {FontFamily} |");
        builder.AppendLine($"| {nameof(FontSize)} | {FontSize} |");
        builder.AppendLine($"| {nameof(RelativeFilePath)} | {RelativeFilePath} |");
        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }
}
