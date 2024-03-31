using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Checklist;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class TaskViewModel : ObservableObject
{
    private readonly ChecklistWidget _widget;
    private readonly SolidColorBrush _titleDefaultForeground;
    private readonly SolidColorBrush _titleCompletedForeground;

    public TaskViewModel(
        ChecklistWidget widget,
        string title,
        DateTimeOffset? dueDate,
        SolidColorBrush titleDefaultForeground,
        SolidColorBrush titleCompletedForeground,
        SolidColorBrush dueDateBackground,
        SolidColorBrush dueDateDueBackground,
        SolidColorBrush dueDateAlmostDueBackground)
    {
        _widget = widget;
        _titleDefaultForeground = titleDefaultForeground;
        _titleCompletedForeground = titleCompletedForeground;

        Title = title;
        DueDate = dueDate.HasValue ? dueDate.Value.ToString("MMM d", Globals.Culture) : string.Empty;
        DueDateVisibility = dueDate.HasValue ? Visibility.Visible : Visibility.Collapsed;

        DueDateBackground = dueDateBackground;
        if (dueDate.HasValue)
        {
            var now = DateTime.Now;
            if (dueDate.Value.Date <= now.Date)
            {
                DueDateTooltip = dueDate.Value.Date == now.Date ? "Due today" : "Overdue";
                DueDateBackground = dueDateDueBackground;
            }
            else if (dueDate.Value.Date == now.Date.AddDays(1))
            {
                DueDateTooltip = "Due tomorrow";
                DueDateBackground = dueDateAlmostDueBackground;
            }
        }
    }

    public string Title { get; }
    public SolidColorBrush TitleForeground => IsEnabled ? _titleDefaultForeground : _titleCompletedForeground;
    public string DueDate { get; }
    public Visibility DueDateVisibility { get; }
    public string? DueDateTooltip { get; }
    public SolidColorBrush DueDateBackground { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForeground))]
    private bool isEnabled = true;

    public void Complete()
    {
        IsEnabled = false;

        _widget.CompleteTask(Title);
    }
}
