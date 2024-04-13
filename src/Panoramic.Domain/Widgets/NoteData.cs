using System.Text;

namespace Panoramic.Data.Widgets;

public sealed class NoteData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public HeaderHighlight HeaderHighlight { get; init; }
    public string FontFamily { get; init; } = "Default";
    public double FontSize { get; init; } = 15;
    public string? RelativeFilePath { get; init; }

    public static NoteData FromMarkdown(string markdown)
    {
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        var lineIndex = 6;

        // Metadata
        var idRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var areaRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var headerHighlightRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var fontFamilyRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var fontSizeRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var relativeFilePathRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new NoteData
        {
            Id = Guid.ParseExact(idRowValues[1], "N"),
            Area = new(areaRowValues[1]),
            HeaderHighlight = Enum.Parse<HeaderHighlight>(headerHighlightRowValues[1]),
            FontFamily = fontFamilyRowValues[1],
            FontSize = double.Parse(fontSizeRowValues[1]),
            RelativeFilePath = relativeFilePathRowValues.Length == 1 ? null : relativeFilePathRowValues[1]
        };
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
