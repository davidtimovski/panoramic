using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Data;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class TaskViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly ChecklistWidget _widget;
    private readonly SolidColorBrush _titleDefaultForeground;
    private readonly SolidColorBrush _titleCompletedForeground;

    public TaskViewModel(
        IEventHub eventHub,
        ChecklistWidget widget,
        string title,
        DateTimeOffset? dueDate,
        Uri? uri,
        SolidColorBrush titleDefaultForeground,
        SolidColorBrush titleCompletedForeground,
        SolidColorBrush dueDateBackground,
        SolidColorBrush dueDateOverdueBackground,
        SolidColorBrush dueDateAlmostDueBackground)
    {
        _eventHub = eventHub;
        _widget = widget;
        _titleDefaultForeground = titleDefaultForeground;
        _titleCompletedForeground = titleCompletedForeground;

        Title = title;
        DueDateVisibility = dueDate.HasValue ? Visibility.Visible : Visibility.Collapsed;

        DueDateBackground = dueDateBackground;
        if (dueDate.HasValue)
        {
            var now = DateTime.Now;
            if (dueDate.Value.Date <= now.Date)
            {
                DueDateBackground = dueDateOverdueBackground;
            }
            else if (dueDate.Value.Date == now.Date.AddDays(1))
            {
                DueLabel = "Tomorrow";
                DueDateBackground = dueDateAlmostDueBackground;
            }
            else
            {
                DueLabel = dueDate.Value.ToString("MMM d", Global.Culture);
            }
        }

        Uri = uri;
    }

    public string Title { get; }
    public SolidColorBrush TitleForeground => IsEnabled ? _titleDefaultForeground : _titleCompletedForeground;
    public string DueLabel { get; } = string.Empty;
    public Visibility DueDateVisibility { get; }
    public SolidColorBrush DueDateBackground { get; }
    public Uri? Uri { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForeground))]
    private bool isEnabled = true;

    public void Complete()
    {
        IsEnabled = false;

        _widget.CompleteTask(Title);
    }

    public void Clicked() => _eventHub.RaiseHyperlinkClicked($"{Title} - {_widget.Title}", Uri!, DateTime.Now);
}
