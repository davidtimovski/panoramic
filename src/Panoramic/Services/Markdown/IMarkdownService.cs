using System.Collections.Generic;
using Microsoft.UI.Xaml.Documents;

namespace Panoramic.Services.Markdown;

public interface IMarkdownService
{
    IReadOnlyList<Paragraph> TextToMarkdownParagraphs(string text, string noteName, double fontSize);
}
