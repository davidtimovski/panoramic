using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Services.Search;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class ChecklistViewModel : WidgetViewModel
{
    private static readonly TimeSpan TaskRemovalDebounceInterval = TimeSpan.FromMilliseconds(650);

    private readonly SolidColorBrush _titleDefaultForeground;
    private readonly SolidColorBrush _titleCompletedForeground;
    private readonly SolidColorBrush _taskDueDateBackground;
    private readonly SolidColorBrush _taskDueDateOverdueBackground;
    private readonly SolidColorBrush _taskDueDateAlmostDueBackground;

    private readonly ISearchService _searchService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly DispatcherQueueTimer _debounceTimer;
    private readonly ChecklistWidget _widget;
    private readonly Queue<TaskViewModel> _tasksToBeRemoved = [];

    public ChecklistViewModel(ISearchService searchService, DispatcherQueue dispatcherQueue, Page page, ChecklistWidget widget)
    {
        _titleDefaultForeground = ResourceUtil.WidgetForeground;
        _titleCompletedForeground = ResourceUtil.HighlightedForeground;
        _taskDueDateBackground = ResourceUtil.HighlightedBackground;
        _taskDueDateOverdueBackground = ResourceUtil.GetBrushFromPage("PanoramicTaskOverdueBackgroundBrush", page);
        _taskDueDateAlmostDueBackground = ResourceUtil.GetBrushFromPage("PanoramicTaskAlmostDueBackgroundBrush", page);

        _searchService = searchService;
        if (widget.Searchable)
        {
            _searchService.SearchInvoked += SearchInvoked;
        }

        _dispatcherQueue = dispatcherQueue;
        _debounceTimer = dispatcherQueue.CreateTimer();

        _widget = widget;
        _widget.TaskAdded += TaskAdded;
        _widget.TaskCompleted += TaskCompleted;

        HeaderBackgroundBrush = ResourceUtil.WidgetHeaderBrushes[widget.HeaderHighlight];
        Title = widget.Title;

        SetViewModel();
    }

    public SolidColorBrush HeaderBackgroundBrush { get; }

    public string Title { get; }

    public readonly ObservableCollection<TaskViewModel> Tasks = [];

    [ObservableProperty]
    private Visibility filterIconVisibility = Visibility.Collapsed;

    private void SearchInvoked(object? _, EventArgs e) => _dispatcherQueue.TryEnqueue(SetViewModel);

    private void SetViewModel()
    {
        var source = _widget.Tasks.AsEnumerable();
        if (_searchService.SearchText.Length > 0)
        {
            source = source.Where(x => x.Matches(_searchService.SearchText));

            FilterIconVisibility = Visibility.Visible;
        }
        else
        {
            FilterIconVisibility = Visibility.Collapsed;
        }

        var filteredTaskVms = source.Select(MapToViewModel).ToList();

        Tasks.Clear();
        foreach (var taskVm in filteredTaskVms)
        {
            Tasks.Add(taskVm);
        }
    }

    private void TaskAdded(object? _, EventArgs e) => SetViewModel();

    private void TaskCompleted(object? _, TaskCompletedEventArgs e)
    {
        var completedTaskVm = Tasks.FirstOrDefault(x => x.Title.Equals(e.Title, StringComparison.OrdinalIgnoreCase));
        if (completedTaskVm is not null)
        {
            _tasksToBeRemoved.Enqueue(completedTaskVm);
        }

        _debounceTimer.Debounce(() =>
        {
            while (_tasksToBeRemoved.Count > 0)
            {
                var taskVm = _tasksToBeRemoved.Dequeue();
                Tasks.Remove(taskVm);
            }
        }, TaskRemovalDebounceInterval);
    }

    private TaskViewModel MapToViewModel(ChecklistTask task)
    {
        var dueDate = task.DueDate.HasValue ? (DateTimeOffset?)task.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        return new(_widget, task.Title, dueDate, _titleDefaultForeground, _titleCompletedForeground, _taskDueDateBackground, _taskDueDateOverdueBackground, _taskDueDateAlmostDueBackground);
    }
}
