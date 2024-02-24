using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;

namespace Panoramic.Services.Markdown;

public sealed partial class MarkdownService(IEventHub eventHub) : IMarkdownService
{
    private const int ParagraphMarginBottom = 20;
    private const int HeaderMarginBottom = 15;
    private const int BulletPointMarginLeft = 3;
    private const int BulletPointMarginBottom = 6;
    private const int NestedBulletPointMarginLeft = 12;

    private readonly IEventHub _eventHub = eventHub;

    public IReadOnlyList<Paragraph> TextToMarkdownParagraphs(string text, string noteName, double fontSize)
    {
        var lines = text.Split('\r').Where(x => x.Trim().Length > 0).ToList();
        var result = new List<Paragraph>(lines.Count);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (IsHeader1(line))
            {
                result.Add(ToHeader1(line, noteName, fontSize));
            }
            else if (IsHeader2(line))
            {
                result.Add(ToHeader2(line, noteName, fontSize));
            }
            else if (IsHeader3(line))
            {
                result.Add(ToHeader3(line, noteName, fontSize));
            }
            else if (IsHeader4(line))
            {
                result.Add(ToHeader4(line, noteName, fontSize));
            }
            else if (IsHeader5(line))
            {
                result.Add(ToHeader5(line, noteName, fontSize));
            }
            else if (IsHeader6(line))
            {
                result.Add(ToHeader6(line, noteName, fontSize));
            }
            else if (IsBulletPoint(line))
            {
                var lastBulletPoint = true;
                if (i < lines.Count - 1)
                {
                    var nextLine = lines[i + 1];
                    lastBulletPoint = !(IsBulletPoint(nextLine) || IsNestedBulletPoint(nextLine));
                }

                result.Add(ToBulletPoint(line, lastBulletPoint, noteName, fontSize));
            }
            else if (IsNestedBulletPoint(line))
            {
                var lastBulletPoint = true;
                if (i < lines.Count - 1)
                {
                    var nextLine = lines[i + 1];
                    lastBulletPoint = !(IsBulletPoint(nextLine) || IsNestedBulletPoint(nextLine));
                }

                result.Add(ToNestedBulletPoint(line, lastBulletPoint, noteName, fontSize));
            }
            else
            {
                result.Add(ToNormalText(line, noteName, fontSize));
            }
        }

        return result;
    }

    private Paragraph ToNormalText(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize,
            Margin = new Thickness(0, 0, 0, ParagraphMarginBottom)
        };

        ParseUrisAndAddToParagraph(text, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader1(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize + 10,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[2..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader2(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize + 8,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[3..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader3(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize + 6,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[4..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader4(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize + 4,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[5..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader5(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize + 2,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[6..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToHeader6(string text, string noteName, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, HeaderMarginBottom)
        };

        var formattedText = text[7..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private Paragraph ToBulletPoint(string text, bool isLast, string noteName, double fontSize)
    {
        var marginBottom = isLast ? ParagraphMarginBottom : BulletPointMarginBottom;
        var paragraph = new Paragraph
        {
            FontSize = fontSize,
            Margin = new Thickness(BulletPointMarginLeft, 0, 0, marginBottom)
        };

        var formattedText = "• " + text[2..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private static bool IsHeader1(string text) => text.StartsWith("# ");
    private static bool IsHeader2(string text) => text.StartsWith("## ");
    private static bool IsHeader3(string text) => text.StartsWith("### ");
    private static bool IsHeader4(string text) => text.StartsWith("#### ");
    private static bool IsHeader5(string text) => text.StartsWith("##### ");
    private static bool IsHeader6(string text) => text.StartsWith("###### ");
    private static bool IsBulletPoint(string text) => text.StartsWith("- ") || text.StartsWith("* ") || text.StartsWith("+ ");
    private static bool IsNestedBulletPoint(string text) => text.StartsWith("  - ") || text.StartsWith("  * ") || text.StartsWith("  + ");

    private Paragraph ToNestedBulletPoint(string text, bool isLast, string noteName, double fontSize)
    {
        var marginBottom = isLast ? ParagraphMarginBottom : BulletPointMarginBottom;
        var paragraph = new Paragraph
        {
            FontSize = fontSize,
            Margin = new Thickness(NestedBulletPointMarginLeft, 0, 0, marginBottom)
        };

        var formattedText = "◦ " + text[4..];

        ParseUrisAndAddToParagraph(formattedText, paragraph, noteName, fontSize);
        return paragraph;
    }

    private static Queue<StringSegment> FindUris(string text)
    {
        var result = new Queue<StringSegment>();

        var match = MyRegex().Match(text);
        while (match.Success)
        {
            var fullGroup = match.Groups[0]!;
            var textGroup = match.Groups[1]!;
            var uriGroup = match.Groups[2]!;

            result.Enqueue(new StringSegment
            {
                Uri = new Uri(uriGroup.Value, UriKind.Absolute),
                Text = textGroup.Value,
                StartIndex = fullGroup.Index,
                Length = fullGroup.Length
            });

            match = match.NextMatch();
        }

        return result;
    }

    private void ParseUrisAndAddToParagraph(string text, Paragraph paragraph, string noteName, double fontSize)
    {
        var uris = FindUris(text);
        if (uris.Count == 0)
        {
            paragraph.Inlines.Add(new Run { Text = text });
            return;
        }

        var currentIndex = 0;
        while (uris.TryDequeue(out var uri))
        {
            if (uri.StartIndex != currentIndex)
            {
                var run = new Run { Text = text[currentIndex..uri.StartIndex] };
                paragraph.Inlines.Add(run);
            }

            var hyperlink = new Hyperlink
            {
                FontSize = fontSize,
                NavigateUri = uri.Uri
            };
            hyperlink.Click += (_, _) => { _eventHub.RaiseHyperlinkClicked($"{noteName} - {uri.Text}", uri.Uri, DateTime.Now); };
            hyperlink.Inlines.Add(new Run { Text = uri.Text });
            paragraph.Inlines.Add(hyperlink);

            currentIndex = uri.StartIndex + uri.Length;
        }

        if (currentIndex < text.Length)
        {
            var run = new Run { Text = text[currentIndex..] };
            paragraph.Inlines.Add(run);
        }
    }

    [GeneratedRegex("\\[([^\\[\\]]*)\\]\\((.*?)\\)")]
    private static partial Regex MyRegex();

    private sealed class StringSegment
    {
        internal required Uri Uri { get; init; }
        internal required string Text { get; init; }
        internal required int StartIndex { get; init; }
        internal required int Length { get; init; }
    }
}
