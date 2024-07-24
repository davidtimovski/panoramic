using System.Text;
using System.Text.RegularExpressions;
using Panoramic.Data.Exceptions;

namespace Panoramic.Data.Widgets;

public sealed partial class ChecklistData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public HighlightColor HeaderHighlight { get; init; }
    public string Title { get; init; } = "To do";
    public bool Searchable { get; init; } = true;
    public required List<ChecklistTaskData> Tasks { get; init; }

    public static ChecklistData FromMarkdown(string markdown)
    {
        var lineIndex = 0;
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        try
        {
            var title = lines[lineIndex][2..];

            lineIndex += 2;

            // Tasks
            var tasks = new List<ChecklistTaskData>();
            while (lines[lineIndex].StartsWith('-'))
            {
                Uri? uri = null;
                var taskTitle = lines[lineIndex][6..];

                var match = HyperlinkMarkdownRegex().Match(taskTitle);
                if (match.Success)
                {
                    var titleGroup = match.Groups[1];
                    var uriGroup = match.Groups[2];

                    taskTitle = titleGroup.Value;
                    uri = new Uri(uriGroup.Value);
                }

                var taskDueDateString = lines[lineIndex + 1][6..].Trim();
                var taskDueDate = taskDueDateString.Length > 0 ? DateOnly.ParseExact(taskDueDateString, Global.StoredDateOnlyFormat) : (DateOnly?)null;
                var taskCreatedString = lines[lineIndex + 2][11..];
                var taskCreated = DateTime.ParseExact(taskCreatedString, Global.StoredDateTimeFormat, Global.Culture);

                tasks.Add(new ChecklistTaskData
                {
                    Title = taskTitle,
                    DueDate = taskDueDate,
                    Uri = uri,
                    Created = taskCreated
                });

                lineIndex += 3;
            }

            lineIndex += 5;

            // Metadata
            var idRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var id = Guid.ParseExact(idRowValues[1], "N");
            lineIndex++;

            var areaRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var area = new Area(areaRowValues[1]);
            lineIndex++;

            var headerHighlightRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var headerHighlight = Enum.Parse<HighlightColor>(headerHighlightRowValues[1]);
            lineIndex++;

            var searchableRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var searchable = bool.Parse(searchableRowValues[1]);

            return new ChecklistData
            {
                Id = id,
                Area = area,
                HeaderHighlight = headerHighlight,
                Title = title,
                Searchable = searchable,
                Tasks = tasks
            };
        }
        catch
        {
            throw new MarkdownParsingException(lines, lineIndex);
        }
    }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"# {Title}");
        builder.AppendLine();

        foreach (var task in Tasks)
        {
            task.ToMarkdown(builder);
        }
        builder.AppendLine();

        builder.AppendLine($"## Metadata");
        builder.AppendLine();

        var metadata = new Dictionary<string, string>
        {
            { nameof(Id), Id.ToString("N") },
            { nameof(Area), Area.ToString() },
            { nameof(HeaderHighlight), HeaderHighlight.ToString() },
            { nameof(Searchable), Searchable.ToString() }
        };

        MarkdownUtil.CreateKeyValueTable(builder, metadata);

        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }

    [GeneratedRegex(@"\[([^\[\]]*)\]\((.*?)\)", RegexOptions.Singleline)]
    private static partial Regex HyperlinkMarkdownRegex();
}

public sealed class ChecklistTaskData
{
    public required string Title { get; init; }
    public required DateOnly? DueDate { get; init; }
    public required Uri? Uri { get; init; }
    public required DateTime Created { get; init; }

    public void ToMarkdown(StringBuilder builder)
    {
        var title = Uri is not null ? $"[{Title}]({Uri})" : Title;
        builder.AppendLine($"- [ ] {title}");

        var dueDate = DueDate.HasValue ? DueDate.Value.ToString(Global.StoredDateOnlyFormat) : string.Empty;
        builder.AppendLine($"  Due: {dueDate}");

        builder.AppendLine($"  Created: {Created.ToString(Global.StoredDateTimeFormat)}");
    }
}
