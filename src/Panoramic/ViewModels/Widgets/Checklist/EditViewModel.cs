using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Events;
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
            var vm = new EditTaskViewModel(task.Title, dueDate, task.Uri, task.Created);
            vm.Updated += (object? _, EventArgs e) => { ValidateAndEmit(); };

            Tasks.Add(vm);
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewTaskFormValid))]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? newTaskDueDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewTaskFormValid))]
    private string newTaskUrl = string.Empty;

    public bool NewTaskFormValid =>
        NewTaskTitle.Trim().Length > 0 &&
        (NewTaskUrl.Trim().Length == 0 || Uri.TryCreate(NewTaskUrl.Trim(), UriKind.Absolute, out var _));

    public ObservableCollection<EditTaskViewModel> Tasks { get; } = [];

    public bool TaskExists() => Tasks.Any(x => string.Equals(x.Title, NewTaskTitle, StringComparison.Ordinal));

    public void Add()
    {
        var uri = NewTaskUrl.Trim().Length > 0 && Uri.TryCreate(NewTaskUrl.Trim(), UriKind.Absolute, out var createdUri) ? createdUri : null;

        Tasks.Add(new EditTaskViewModel(NewTaskTitle.Trim(), NewTaskDueDate, uri, DateTime.Now));

        NewTaskTitle = string.Empty;
        NewTaskDueDate = null;
    }

    public void Delete(EditTaskViewModel viewModel)
    {
        Tasks.Remove(viewModel);
        ValidateAndEmit();
    }

    public async Task SaveAsync()
    {
        _widget.Tasks = Tasks
            .Select(x => new ChecklistTask
            {
                Title = x.Title.Trim(),
                DueDate = x.DueDate.HasValue ? DateOnly.FromDateTime(x.DueDate.Value.Date) : null,
                Uri = x.Uri,
                Created = x.Created
            }).ToList();

        await _storageService.SaveWidgetAsync(_widget);
    }

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = Tasks.All(x => x.IsValid()) });
}
