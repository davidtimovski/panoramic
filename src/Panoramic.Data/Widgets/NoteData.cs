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
    public bool Editing { get; init; }
    public List<string> RecentNotes { get; init; } = [];
    public int RecentNotesCapacity { get; init; } = 5;

    public static NoteData FromMarkdown(string markdown)
    {
        var lineIndex = 3;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
            // RecentNotes
            var recentNotes = new List<string>();
            while (lines[lineIndex].StartsWith('-'))
            {
                var recentNote = lines[lineIndex][2..];
                recentNotes.Add(recentNote);

                lineIndex++;
            }

            lineIndex += 5;

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
            lineIndex++;

            var editingRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var editing = bool.Parse(editingRowValues[1]);
            lineIndex++;

            var recentNotesCapacityRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var recentNotesCapacity = int.Parse(recentNotesCapacityRowValues[1]);

            return new NoteData
            {
                Id = id,
                Area = area,
                HeaderHighlight = headerHighlight,
                FontFamily = fontFamily,
                FontSize = fontSize,
                RelativeFilePath = relativeFilePath,
                Editing = editing,
                RecentNotes = recentNotes,
                RecentNotesCapacity = recentNotesCapacity,
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

        builder.AppendLine($"## Recent notes");
        foreach (var recentNotePath in RecentNotes)
        {
            builder.AppendLine($"- {recentNotePath}");
        }
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();

        var metadata = new Dictionary<string, string>
        {
            { nameof(Id), Id.ToString("N") },
            { nameof(Area), Area.ToString() },
            { nameof(HeaderHighlight), HeaderHighlight.ToString() },
            { nameof(FontFamily), FontFamily },
            { nameof(FontSize), FontSize.ToString() },
            { nameof(RelativeFilePath), RelativeFilePath is null ? string.Empty : RelativeFilePath },
            { nameof(Editing), Editing.ToString() },
            { nameof(RecentNotesCapacity), RecentNotesCapacity.ToString() },
        };

        MarkdownUtil.CreateKeyValueTable(builder, metadata);

        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }
}
