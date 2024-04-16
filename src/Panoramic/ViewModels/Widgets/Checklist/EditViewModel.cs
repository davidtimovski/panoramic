using System;
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
    private readonly ChecklistWidget _widget;

    public EditViewModel(
        IStorageService storageService,
        ChecklistWidget widget)
    {
        _storageService = storageService;
        _widget = widget;

        foreach (var task in widget.Tasks)
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
        _widget.Tasks = Tasks
            .Select(x => new ChecklistTask
            {
                Title = x.Title.Trim(),
                DueDate = x.DueDate.HasValue ? DateOnly.FromDateTime(x.DueDate.Value.Date) : null,
                Created = x.Created
            }).ToList();

        await _storageService.SaveWidgetAsync(_widget);
    }
}
