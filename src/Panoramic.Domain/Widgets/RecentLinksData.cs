using System.Text;
using System.Text.RegularExpressions;

namespace Panoramic.Data.Widgets;

public sealed partial class RecentLinksData : IWidgetData
{
    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public string Title { get; init; } = "Recent";
    public int Capacity { get; init; } = 15;
    public bool OnlyFromToday { get; init; }
    public bool Searchable { get; init; } = true;
    public required List<RecentLinkData> Links { get; init; }

    public static RecentLinksData FromMarkdown(string markdown)
    {
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        var lineIndex = 0;
        var title = lines[lineIndex][2..];

        lineIndex += 4;

        // Links
        var links = new List<RecentLinkData>();
        while (lines[lineIndex].StartsWith('|'))
        {
            var linkLineParts = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var hyperlinkMarkdown = linkLineParts[0];
            var match = HyperlinkMarkdownRegex().Match(hyperlinkMarkdown);
            if (!match.Success)
            {
                throw new ApplicationException($"Cannot parse hyperlink markdown: {hyperlinkMarkdown}");
            }

            var titleGroup = match.Groups[1]!;
            var uriGroup = match.Groups[2]!;

            links.Add(new RecentLinkData
            {
                Title = titleGroup.Value,
                Uri = new Uri(uriGroup.Value),
                Clicked = DateTime.Parse(linkLineParts[1])
            });

            lineIndex++;
        }

        lineIndex += 5;

        // Metadata
        var idRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var areaRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var capacityRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var onlyFromTodayRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var searchableRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new RecentLinksData
        {
            Id = Guid.ParseExact(idRowValues[1], "N"),
            Area = new(areaRowValues[1]),
            Title = title,
            Capacity = int.Parse(capacityRowValues[1]),
            OnlyFromToday = bool.Parse(onlyFromTodayRowValues[1]),
            Searchable = bool.Parse(searchableRowValues[1]),
            Links = links
        };
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# {Title}");
        builder.AppendLine();

        builder.AppendLine("| Link | Clicked |");
        builder.AppendLine("| - | - |");

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
        builder.AppendLine($"| {nameof(Capacity)} | {Capacity} |");
        builder.AppendLine($"| {nameof(OnlyFromToday)} | {OnlyFromToday} |");
        builder.AppendLine($"| {nameof(Searchable)} | {Searchable} |");
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class RecentLinkData
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; init; }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"| [{Title}]({Uri}) | {Clicked} |");
    }
}
