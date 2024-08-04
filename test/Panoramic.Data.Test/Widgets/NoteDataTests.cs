using System.Text;
using System.Text.Json;
using Panoramic.Data.Widgets;

namespace Panoramic.Data.Test.Widgets;

public class NoteDataTests
{
    private static readonly NoteData Sut = new()
    {
        Id = Guid.NewGuid(),
        Area = new Area("03-24"),
        HeaderHighlight = HighlightColor.Green,
        FontFamily = "Default",
        FontSize = 15,
        RelativeFilePath = "My note.md",
        Editing = true,
        RecentNotes =
        [
            "Another note.md",
            "A third note.md"
        ],
        RecentNotesCapacity = 5
    };

    [Fact]
    public void MarkdownSerializationProducesExpectedResult()
    {
        var expectedData = JsonSerializer.Serialize(Sut);

        var builder = new StringBuilder();
        Sut.ToMarkdown(builder);
        string asMarkdown = builder.ToString();

        NoteData asObject = NoteData.FromMarkdown(string.Empty, asMarkdown);
        var actualData = JsonSerializer.Serialize(asObject);

        Assert.Equal(expectedData, actualData);
    }
}
