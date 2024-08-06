using System.Text;
using System.Text.RegularExpressions;
using Panoramic.Data.Exceptions;

namespace Panoramic.Data;

public sealed partial class LinkDrawerData
{
    public required string Name { get; init; }
    public required IReadOnlyList<LinkDrawerLinkData> Links { get; init; }

    public static LinkDrawerData FromMarkdown(string relativeFilePath, string markdown)
    {
        var lineIndex = 2;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
            short order = 0;
            var links = new List<LinkDrawerLinkData>();
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
                var searchTerms = linkLineParts.Length == 2 ? linkLineParts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() : [];

                links.Add(new LinkDrawerLinkData
                {
                    Title = titleGroup.Value,
                    Uri = new Uri(uriGroup.Value),
                    Order = order,
                    SearchTerms = searchTerms
                });

                order++;
                lineIndex++;
            }

            var name = Path.GetFileNameWithoutExtension(relativeFilePath);

            return new LinkDrawerData
            {
                Name = name,
                Links = links
            };
        }
        catch
        {
            throw new MarkdownParsingException(relativeFilePath, lines, lineIndex);
        }
    }

    public void ToMarkdown(StringBuilder builder)
    {
        var headers = new Tuple<string, string>("Link", "Search terms");
        var data = Links.Select(x => new Tuple<string, string>($"[{x.Title}]({x.Uri})", string.Join(LinkDrawerLinkData.SearchTermsSeparator, x.SearchTerms))).ToList();

        MarkdownUtil.CreateTwoColumnTable(builder, headers, data);
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class LinkDrawerLinkData
{
    public const string SearchTermsSeparator = ", ";

    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }
    public required List<string> SearchTerms { get; init; }

    /// <summary>
    /// Used for link search functionality.
    /// </summary>
    public WeighedSearchResult<LinkDrawerLinkData> Matches(string searchText)
    {
        var weight = 0;

        if (Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            weight += 1;
        }

        if (Uri.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            weight += 1;
        }

        weight += SearchTerms.Count(x => x.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        return new WeighedSearchResult<LinkDrawerLinkData>
        {
            Weight = weight,
            Result = this
        };
    }
}

public sealed class WeighedSearchResult<T>
{
    public required int Weight { get; init; }
    public required T Result { get; init; }
}
