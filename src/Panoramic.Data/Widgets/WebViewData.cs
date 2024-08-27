using System.Text;
using Panoramic.Data.Exceptions;

namespace Panoramic.Data.Widgets;

public sealed class WebViewData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public HighlightColor HeaderHighlight { get; init; }
    public string Title { get; init; } = "Web page";
    public Uri Uri { get; init; } = new("https://www.example.com");

    public static WebViewData FromMarkdown(string relativeFilePath, string markdown)
    {
        var lineIndex = 0;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
            var title = lines[lineIndex][2..];

            lineIndex += 6;

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

            var uriRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var uri = uriRowValues[1];

            return new WebViewData
            {
                Id = id,
                Area = area,
                HeaderHighlight = headerHighlight,
                Title = title,
                Uri = new Uri(uri)
            };
        }
        catch
        {
            throw new MarkdownParsingException(relativeFilePath, lines, lineIndex);
        }
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# {Title}");
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();

        var metadataHeaders = new Tuple<string, string>("Key", "Value");
        var metadata = new List<Tuple<string, string>>
        {
            new(nameof(Id), Id.ToString("N")),
            new(nameof(Area), Area.ToString()),
            new(nameof(HeaderHighlight), HeaderHighlight.ToString()),
            new(nameof(Uri), Uri.ToString())
        };

        MarkdownUtil.CreateTwoColumnTable(builder, metadataHeaders, metadata);

        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }
}
