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
                var searchTerms = linkLineParts.Length == 2 ? linkLineParts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];

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
        var data = Links.Select(x => new Tuple<string, string>($"[{x.Title}]({x.Uri})", string.Join(", ", x.SearchTerms))).ToList();

        MarkdownUtil.CreateTwoColumnTable(builder, headers, data);
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class LinkDrawerLinkData
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }
    public required IReadOnlyCollection<string> SearchTerms { get; init; }

    /// <summary>
    /// Used for link search functionality.
    /// </summary>
    public bool Matches(string searchText)
        => Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) || Uri.Host.Contains(searchText, StringComparison.OrdinalIgnoreCase);
}
