using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.Checklist;

public sealed class ChecklistWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new link collection widget.
    /// </summary>
    public ChecklistWidget(IStorageService storageService, Area area, HighlightColor headerHighlight, string title, bool searchable)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.Checklist);

        Area = area;
        HeaderHighlight = headerHighlight;
        Title = title;
        Searchable = searchable;
        tasks = [];
    }

    /// <summary>
    /// Constructs a link collection widget based on existing data.
    /// </summary>
    private ChecklistWidget(IStorageService storageService, ChecklistData data)
    {
        _storageService = storageService;
        _dataFileName = WidgetUtil.CreateDataFileName(data.Id, WidgetType.Checklist);

        Id = data.Id;
        Area = data.Area;
        HeaderHighlight = data.HeaderHighlight;
        Title = data.Title;
        Searchable = data.Searchable;
        tasks = data.Tasks.Select(x => new ChecklistTask { Title = x.Title, DueDate = x.DueDate, Uri = x.Uri, Created = x.Created }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.Checklist;
    public Area Area { get; set; }
    public HighlightColor HeaderHighlight { get; set; }
    public string Title { get; set; }
    public bool Searchable { get; set; }

    private List<ChecklistTask> tasks;
    public IReadOnlyList<ChecklistTask> Tasks
    {
        get => [.. tasks.OrderBy(SortTask)];
        set
        {
            tasks = [.. value];
        }
    }

    public event EventHandler<EventArgs>? TaskAdded;
    public event EventHandler<TaskCompletedEventArgs>? TaskCompleted;

    public bool TaskCanBeCreated(string title)
        => !tasks.Any(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

    public void AddTask(string title, DateOnly? dueDate, Uri? uri)
    {
        tasks.Add(new ChecklistTask { Title = title, DueDate = dueDate, Uri = uri, Created = DateTime.Now });
        _storageService.EnqueueWidgetWrite(Id);

        TaskAdded?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteTask(string title)
    {
        tasks.RemoveAll(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        _storageService.EnqueueWidgetWrite(Id);

        TaskCompleted?.Invoke(this, new TaskCompletedEventArgs { Title = title });
    }

    public ChecklistData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            HeaderHighlight = HeaderHighlight,
            Title = Title,
            Searchable = Searchable,
            Tasks = tasks.Select(x => new ChecklistTaskData { Title = x.Title, DueDate = x.DueDate, Uri = x.Uri, Created = x.Created }).ToList()
        };

    public static ChecklistWidget Load(IStorageService storageService, string markdown)
    {
        var data = ChecklistData.FromMarkdown(markdown);
        return new(storageService, data);
    }

    public async Task WriteAsync()
    {
        DebugLogger.Log($"Writing {Type} widget with ID: {Id}");

        var data = GetData();

        var builder = new StringBuilder();
        data.ToMarkdown(builder);
        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), builder.ToString());
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }

    private static long SortTask(ChecklistTask task)
        => task.DueDate.HasValue
            ? task.DueDate.Value.ToDateTime(TimeOnly.MinValue).Ticks
            : task.Created.Ticks * 2;
}
