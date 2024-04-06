using System.Text;
using System.Text.RegularExpressions;

namespace Panoramic.Data.Widgets;

public sealed partial class LinkCollectionData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public string Title { get; init; } = "My links";
    public bool Searchable { get; init; } = true;
    public required List<LinkCollectionItemData> Links { get; init; }

    public static LinkCollectionData FromMarkdown(string markdown)
    {
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        var lineIndex = 0;
        var title = lines[lineIndex][2..];

        lineIndex += 4;

        // Links
        short order = 0;
        var links = new List<LinkCollectionItemData>();
        while (lines[lineIndex].StartsWith('-'))
        {
            var hyperlinkMarkdown = lines[lineIndex][2..];
            var match = HyperlinkMarkdownRegex().Match(hyperlinkMarkdown);
            if (!match.Success)
            {
                throw new ApplicationException($"Cannot parse hyperlink markdown: {hyperlinkMarkdown}");
            }

            var titleGroup = match.Groups[1]!;
            var uriGroup = match.Groups[2]!;

            links.Add(new LinkCollectionItemData
            {
                Title = titleGroup.Value,
                Uri = new Uri(uriGroup.Value),
                Order = order
            });

            order++;
            lineIndex++;
        }

        lineIndex += 5;

        // Metadata
        var idRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var areaRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var searchableRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new LinkCollectionData
        {
            Id = Guid.ParseExact(idRowValues[1], "N"),
            Area = new(areaRowValues[1]),
            Title = title,
            Searchable = bool.Parse(searchableRowValues[1]),
            Links = links
        };
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# {Title}");
        builder.AppendLine();

        foreach (var link in Links)
        {
            link.ToMarkdown(builder);
        }
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();
        builder.AppendLine("| Key | Value |");
        builder.AppendLine("| - | - |");
        builder.AppendLine($"| {nameof(Id)} | {Id:N} |");
        builder.AppendLine($"| {nameof(Area)} | {Area} |");
        builder.AppendLine($"| {nameof(Searchable)} | {Searchable} |");
        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class LinkCollectionItemData
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"- [{Title}]({Uri})");
    }
}
