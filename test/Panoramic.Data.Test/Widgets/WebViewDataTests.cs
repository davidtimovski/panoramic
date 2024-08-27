using System.Text;
using System.Text.Json;
using Panoramic.Data.Widgets;

namespace Panoramic.Data.Test.Widgets;

public class WebViewDataTests
{
    private static readonly WebViewData Sut = new()
    {
        Id = Guid.NewGuid(),
        Area = new Area("35-56"),
        HeaderHighlight = HighlightColor.Orange,
        Title = "Web page",
        Uri = new Uri("https://www.example.com/")
    };

    [Fact]
    public void MarkdownSerializationProducesExpectedResult()
    {
        var expectedData = JsonSerializer.Serialize(Sut);

        var builder = new StringBuilder();
        Sut.ToMarkdown(builder);
        string asMarkdown = builder.ToString();

        WebViewData asObject = WebViewData.FromMarkdown(string.Empty, asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
