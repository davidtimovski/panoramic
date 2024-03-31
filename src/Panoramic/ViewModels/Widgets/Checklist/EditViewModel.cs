using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class EditViewModel : ObservableObject
{
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public EditViewModel(
        IStorageService storageService,
        ChecklistData data)
    {
        _storageService = storageService;
        _id = data.Id;

        foreach (var task in data.Tasks)
        {
            var dueDate = task.DueDate.HasValue ? (DateTimeOffset?)task.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null;
            Tasks.Add(new EditTaskViewModel(task.Title, dueDate, task.Created));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewTaskFormValid))]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? newTaskDueDate;

    public bool NewTaskFormValid => NewTaskTitle.Trim().Length > 0;

    public ObservableCollection<EditTaskViewModel> Tasks { get; } = [];

    public bool TaskExists() => Tasks.Any(x => string.Equals(x.Title, NewTaskTitle, StringComparison.Ordinal));

    public void Add()
    {
        Tasks.Add(new EditTaskViewModel(NewTaskTitle.Trim(), NewTaskDueDate, DateTime.Now));
        NewTaskTitle = string.Empty;
        NewTaskDueDate = null;
    }

    public void Delete(EditTaskViewModel viewModel) => Tasks.Remove(viewModel);

    public async Task SaveAsync()
    {
        var widget = (ChecklistWidget)_storageService.Widgets[_id];

        var tasks = new List<ChecklistTask>(Tasks.Count);
        for (short i = 0; i < Tasks.Count; i++)
        {
            var task = Tasks[i];

            tasks.Add(new ChecklistTask
            {
                Title = task.Title.Trim(),
                DueDate = task.DueDate.HasValue ? DateOnly.FromDateTime(task.DueDate.Value.Date) : null,
                Created = task.Created
            });
        }

        widget.Tasks = tasks;
        await _storageService.SaveWidgetAsync(widget);
    }
}
