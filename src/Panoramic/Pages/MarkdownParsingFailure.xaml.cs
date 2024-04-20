using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Panoramic.Utils;

namespace Panoramic.Pages;

public sealed partial class MarkdownParsingFailure : Page
{
    private static readonly FontFamily MonospacedFontFamily = new("Consolas");

    public MarkdownParsingFailure(string fileName, IReadOnlyList<string> markdownLines, int potentialErrorLine)
    {
        InitializeComponent();

        FileName = fileName;

        for (int i = 0; i < markdownLines.Count; i++)
        {
            var paragraph = new Paragraph { FontFamily = MonospacedFontFamily };

            var line = new Run { Text = markdownLines[i] };
            if (i == potentialErrorLine)
            {
                line.Foreground = ResourceUtil.HighlightedForeground;
            }

            paragraph.Inlines.Add(line);
            MarkdownPresenter.Blocks.Add(paragraph);
        }
    }

    public string FileName { get; }
}
