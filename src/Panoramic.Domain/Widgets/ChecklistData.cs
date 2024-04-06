using System.Text;

namespace Panoramic.Data.Widgets;

public sealed class ChecklistData : IWidgetData
{
    private const short Version = 1;

    public required Guid Id { get; init; }
    public required Area Area { get; init; }
    public string Title { get; init; } = "To do";
    public bool Searchable { get; init; } = true;
    public required List<ChecklistTaskData> Tasks { get; init; }

    public static ChecklistData FromMarkdown(string markdown)
    {
        var lines = markdown.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        var lineIndex = 0;
        var title = lines[lineIndex][2..];

        lineIndex += 2;

        // Tasks
        var tasks = new List<ChecklistTaskData>();
        while (lines[lineIndex].StartsWith('-'))
        {
            var taskTitle = lines[lineIndex][6..];
            var taskDueDateString = lines[lineIndex + 1][6..].Trim();
            var taskDueDate = taskDueDateString.Length > 0 ? DateOnly.ParseExact(taskDueDateString, Global.StoredDateOnlyFormat) : (DateOnly?)null;
            var taskCreatedString = lines[lineIndex + 2][11..];
            var taskCreated = DateTime.ParseExact(taskCreatedString, Global.StoredDateTimeFormat, Global.Culture);

            tasks.Add(new ChecklistTaskData
            {
                Title = taskTitle,
                DueDate = taskDueDate,
                Created = taskCreated
            });

            lineIndex += 3;
        }

        lineIndex += 5;

        // Metadata
        var idRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var areaRowValues = lines[lineIndex++].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var searchableRowValues = lines[lineIndex].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ChecklistData
        {
            Id = Guid.ParseExact(idRowValues[1], "N"),
            Area = new(areaRowValues[1]),
            Title = title,
            Searchable = bool.Parse(searchableRowValues[1]),
            Tasks = tasks
        };
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
        builder.AppendLine("| Key | Value |");
        builder.AppendLine("| - | - |");
        builder.AppendLine($"| {nameof(Id)} | {Id:N} |");
        builder.AppendLine($"| {nameof(Area)} | {Area} |");
        builder.AppendLine($"| {nameof(Searchable)} | {Searchable} |");
        builder.AppendLine();
        builder.Append($"> Version: {Version}");
    }
}

public sealed class ChecklistTaskData
{
    public required string Title { get; init; }
    public required DateOnly? DueDate { get; init; }
    public required DateTime Created { get; init; }

    public void ToMarkdown(StringBuilder builder)
    {
        builder.AppendLine($"- [ ] {Title}");

        var dueDate = DueDate.HasValue ? DueDate.Value.ToString(Global.StoredDateOnlyFormat) : string.Empty;
        builder.AppendLine($"  Due: {dueDate}");

        builder.AppendLine($"  Created: {Created.ToString(Global.StoredDateTimeFormat)}");
    }
}
