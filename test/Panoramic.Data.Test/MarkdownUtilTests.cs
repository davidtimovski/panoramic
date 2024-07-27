using System.Text;

namespace Panoramic.Data.Test;

public class MarkdownUtilTests
{
    [Fact]
    public void CreateKeyValueTable_ProducesCorrectMarkdown()
    {
        // Arrange
        const string ExpectedMarkdown = """
| Key         | Value         |
| ----------- | ------------- |
| SomeKey1    | SomeValue1    |
| SomeKey2    | SomeValue2    |
| SomeKey1000 | SomeValue1000 |

""";

        var tableData = new Dictionary<string, string>
        {
            { "SomeKey1", "SomeValue1" },
            { "SomeKey2", "SomeValue2" },
            { "SomeKey1000", "SomeValue1000" },
        };

        var builder = new StringBuilder();

        // Act
        MarkdownUtil.CreateKeyValueTable(builder, tableData);

        // Assert
        var actualMarkdown = builder.ToString();
        Assert.Equal(ExpectedMarkdown, actualMarkdown);
    }

    [Fact]
    public void CreateThreeColumnTable_ProducesCorrectMarkdown()
    {
        // Arrange
        const string ExpectedMarkdown = """
| Column1          | Column2          | Column3          |
| ---------------- | ---------------- | ---------------- |
| Column1Value1    | Column2Value1    | Column3Value1    |
| Column1Value2    | Column2Value2    | Column3Value2    |
| Column1Value1000 | Column2Value2000 | Column3Value3000 |

""";
        var headers = new Tuple<string, string, string>("Column1", "Column2", "Column3");

        var tableData = new List<Tuple<string, string, string>>
        {
            new("Column1Value1", "Column2Value1", "Column3Value1"),
            new("Column1Value2", "Column2Value2", "Column3Value2"),
            new("Column1Value1000", "Column2Value2000", "Column3Value3000"),
        };

        var builder = new StringBuilder();

        // Act
        MarkdownUtil.CreateThreeColumnTable(builder, headers, tableData);

        // Assert
        var actualMarkdown = builder.ToString();
        Assert.Equal(ExpectedMarkdown, actualMarkdown);
    }
}
