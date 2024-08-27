using System.Text;
using System.Text.Json;

namespace Panoramic.Data.Test;

public class LinkDrawerDataTests
{
    private static readonly LinkDrawerData Sut = new()
    {
        Name = "My links",
        Links =
        [
            new LinkDrawerLinkData
            {
                Title = "Quisque sollicitudin",
                Uri = new Uri("https://www.example.com/?q=1"),
                Order = 0,
                SearchTerms = ["term1", "term2"]
            },
            new LinkDrawerLinkData
            {
                Title = "Vestibulum erat nulla",
                Uri = new Uri("https://www.example.com/?q=2"),
                Order = 1,
                SearchTerms = ["term3", "term4", "term5"]
            }
        ]
    };

    [Fact]
    public void MarkdownSerializationProducesExpectedResult()
    {
        var expectedData = JsonSerializer.Serialize(Sut);

        var builder = new StringBuilder();
        Sut.ToMarkdown(builder);
        string asMarkdown = builder.ToString();

        LinkDrawerData asObject = LinkDrawerData.FromMarkdown($"{Sut.Name}.md", asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
