using System.Text;
using System.Text.Json;
using Panoramic.Data.Widgets;

namespace Panoramic.Data.Test.Widgets;

public class RecentLinksDataTests
{
    private static readonly RecentLinksData Sut = new()
    {
        Id = Guid.NewGuid(),
        Area = new Area("35-56"),
        HeaderHighlight = HighlightColor.Yellow,
        Title = "Recent links",
        Capacity = 15,
        OnlyFromToday = false,
        Searchable = true,
        Links =
        [
            new RecentLinkData
            {
                Title = "Quisque sollicitudin",
                Uri = new Uri("https://www.google.com/search?q=somequeryhere1"),
                Context = "My links",
                Clicked = new DateTime(2024, 4, 4)
            },
            new RecentLinkData
            {
                Title = "Vestibulum erat nulla",
                Uri = new Uri("https://www.google.com/search?q=somequeryhere2"),
                Context = "Other links",
                Clicked = new DateTime(2024, 5, 5)
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

        RecentLinksData asObject = RecentLinksData.FromMarkdown(string.Empty, asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
