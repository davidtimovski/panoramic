using System.Text;

namespace Panoramic.Data;

internal static class MarkdownUtil
{
    /// <summary>
    /// Creates a markdown table with two columns (with headers Key and Value) which are of equal width.
    /// </summary>
    internal static void CreateKeyValueTable(StringBuilder builder, IReadOnlyDictionary<string, string> data)
    {
        if (data.Count == 0)
        {
            builder.AppendLine("| Key | Value |");
            builder.AppendLine("| --- | ----- |");
            return;
        }

        var longestKeyLength = data.Keys.Max(x => x.Length);
        if (longestKeyLength < 3)
        {
            longestKeyLength = 3;
        }

        var longestValueLength = data.Values.Max(x => x.Length);
        if (longestValueLength < 5)
        {
            longestValueLength = 5;
        }

        var keyHeaderContent = $"Key{new string(' ', longestKeyLength - 3)}";
        var valueHeaderContent = $"Value{new string(' ', longestValueLength - 5)}";
        var firstColumnBorderLine = new string('-', longestKeyLength);
        var secondColumnBorderLine = new string('-', longestValueLength);

        builder.AppendLine($"| {keyHeaderContent} | {valueHeaderContent} |");
        builder.AppendLine($"| {firstColumnBorderLine} | {secondColumnBorderLine} |");

        foreach (var field in data)
        {
            var keyContent = field.Key + new string(' ', longestKeyLength - field.Key.Length);
            var valueContent = field.Value + new string(' ', longestValueLength - field.Value.Length);

            builder.AppendLine($"| {keyContent} | {valueContent} |");
        }
    }

    /// <summary>
    /// Creates a markdown table with three columns which are of equal width.
    /// </summary>
    internal static void CreateThreeColumnTable(StringBuilder builder, Tuple<string, string, string> headers, IReadOnlyList<Tuple<string, string, string>> data)
    {
        if (data.Count == 0)
        {
            builder.AppendLine($"| {headers.Item1} | {headers.Item2} | {headers.Item3} |");
            builder.AppendLine($"| {new string('-', headers.Item1.Length)} | {new string('-', headers.Item2.Length)} | {new string('-', headers.Item3.Length)} |");
            return;
        }

        var col1LongestLength = data.Max(x => x.Item1.Length);
        if (col1LongestLength < headers.Item1.Length)
        {
            col1LongestLength = headers.Item1.Length;
        }

        var col2LongestLength = data.Max(x => x.Item2.Length);
        if (col2LongestLength < headers.Item2.Length)
        {
            col2LongestLength = headers.Item2.Length;
        }

        var col3LongestLength = data.Max(x => x.Item3.Length);
        if (col3LongestLength < headers.Item3.Length)
        {
            col3LongestLength = headers.Item3.Length;
        }

        var col1HeaderContent = headers.Item1 + new string(' ', col1LongestLength - headers.Item1.Length);
        var col2HeaderContent = headers.Item2 + new string(' ', col2LongestLength - headers.Item2.Length);
        var col3HeaderContent = headers.Item3 + new string(' ', col3LongestLength - headers.Item3.Length);

        var col1BorderLine = new string('-', col1LongestLength);
        var col2BorderLine = new string('-', col2LongestLength);
        var col3BorderLine = new string('-', col3LongestLength);

        builder.AppendLine($"| {col1HeaderContent} | {col2HeaderContent} | {col3HeaderContent} |");
        builder.AppendLine($"| {col1BorderLine} | {col2BorderLine} | {col3BorderLine} |");

        foreach (var row in data)
        {
            var col1Content = row.Item1 + new string(' ', col1LongestLength - row.Item1.Length);
            var col2Content = row.Item2 + new string(' ', col2LongestLength - row.Item2.Length);
            var col3Content = row.Item3 + new string(' ', col3LongestLength - row.Item3.Length);

            builder.AppendLine($"| {col1Content} | {col2Content} | {col3Content} |");
        }
    }
}
