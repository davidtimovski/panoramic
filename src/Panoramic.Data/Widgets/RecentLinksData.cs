using System.Text;
using System.Text.RegularExpressions;
using Panoramic.Data.Exceptions;

namespace Panoramic.Data.Widgets;

public sealed partial class RecentLinksData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public HighlightColor HeaderHighlight { get; init; }
    public string Title { get; init; } = "Recent";
    public int Capacity { get; init; } = 15;
    public bool OnlyFromToday { get; init; }
    public bool Searchable { get; init; } = true;
    public required List<RecentLinkData> Links { get; init; }

    public static RecentLinksData FromMarkdown(string markdown)
    {
        var lineIndex = 0;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
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

                var titleGroup = match.Groups[1];
                var uriGroup = match.Groups[2];

                links.Add(new RecentLinkData
                {
                    Title = titleGroup.Value,
                    Uri = new Uri(uriGroup.Value),
                    Context = linkLineParts[1],
                    Clicked = DateTime.ParseExact(linkLineParts[2], Global.StoredDateTimeFormat, Global.Culture)
                });

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

            var capacityRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var capacity = int.Parse(capacityRowValues[1]);
            lineIndex++;

            var onlyFromTodayRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var onlyFromToday = bool.Parse(onlyFromTodayRowValues[1]);
            lineIndex++;

            var searchableRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var searchable = bool.Parse(searchableRowValues[1]);

            return new RecentLinksData
            {
                Id = id,
                Area = area,
                HeaderHighlight = headerHighlight,
                Title = title,
                Capacity = capacity,
                OnlyFromToday = onlyFromToday,
                Searchable = searchable,
                Links = links
            };
        }
        catch
        {
            throw new MarkdownParsingException(lines, lineIndex);
        }
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# {Title}");
        builder.AppendLine();

        var headers = new Tuple<string, string, string>("Link", "Context", "Clicked");
        var data = Links.Select(x => new Tuple<string, string, string>($"[{x.Title}]({x.Uri})", x.Context, x.Clicked.ToString(Global.StoredDateTimeFormat))).ToList();

        MarkdownUtil.CreateThreeColumnTable(builder, headers, data);
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();

        var metadata = new Dictionary<string, string>
        {
            { nameof(Id), Id.ToString("N") },
            { nameof(Area), Area.ToString() },
            { nameof(HeaderHighlight), HeaderHighlight.ToString() },
            { nameof(Capacity), Capacity.ToString() },
            { nameof(OnlyFromToday), OnlyFromToday.ToString() },
            { nameof(Searchable), Searchable.ToString() }
        };

        MarkdownUtil.CreateKeyValueTable(builder, metadata);

        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class RecentLinkData
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required string Context { get; init; }
    public required DateTime Clicked { get; init; }
}
