using System.Text;
using System.Text.Json;
using Panoramic.Data.Widgets;

namespace Panoramic.Data.Test.Widgets;

public class ChecklistDataTests
{
    private static readonly ChecklistData Sut = new()
    {
        Id = Guid.NewGuid(),
        Area = new Area("05-26"),
        HeaderHighlight = HighlightColor.Blue,
        Title = "Checklist",
        Searchable = true,
        Tasks =
        [
            new ChecklistTaskData
            {
                Title = "Integer iaculis ex quam",
                DueDate = null,
                Uri = new Uri("https://www.google.com/search?q=somequeryhere1"),
                Created = new DateTime(2024, 4, 4)
            },
            new ChecklistTaskData
            {
                Title = "Morbi imperdiet consectetur",
                DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                Uri = new Uri("https://www.google.com/search?q=somequeryhere2"),
                Created = new DateTime(2024, 5, 5)
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

        ChecklistData asObject = ChecklistData.FromMarkdown(string.Empty, asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
