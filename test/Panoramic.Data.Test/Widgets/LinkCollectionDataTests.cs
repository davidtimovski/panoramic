using System.Text;
using System.Text.Json;
using Panoramic.Data.Widgets;

namespace Panoramic.Data.Test.Widgets;

public class LinkCollectionDataTests
{
    private static readonly LinkCollectionData Sut = new()
    {
        Id = Guid.NewGuid(),
        Area = new Area("33-54"),
        HeaderHighlight = HighlightColor.Red,
        Title = "Link collection",
        Searchable = true,
        Links =
        [
            new LinkCollectionItemData
            {
                Title = "Quisque sollicitudin",
                Uri = new Uri("https://www.example.com/?q=1"),
                Order = 0,
            },
            new LinkCollectionItemData
            {
                Title = "Vestibulum erat nulla",
                Uri = new Uri("https://www.example.com/?q=2"),
                Order = 1,
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

        LinkCollectionData asObject = LinkCollectionData.FromMarkdown(string.Empty, asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
