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
    private const string DueDateFormat = "MMM d";
    private const string DueDateWeekdayFormat = "dddd";

    private readonly IEventHub _eventHub;
    private readonly ChecklistWidget _widget;
    private readonly SolidColorBrush _titleDefaultForeground;
    private readonly SolidColorBrush _titleCompletedForeground;
    private readonly SolidColorBrush _dueDateBackground;
    private readonly SolidColorBrush _dueDateOverdueBackground;
    private readonly SolidColorBrush _dueDateAlmostDueBackground;

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
        _dueDateBackground = dueDateBackground;
        _dueDateOverdueBackground = dueDateOverdueBackground;
        _dueDateAlmostDueBackground = dueDateAlmostDueBackground;

        Title = title;
        DueDate = dueDate;
        DueDateVisibility = dueDate.HasValue ? Visibility.Visible : Visibility.Collapsed;

        DueDateBackground = _dueDateBackground;
        SetDueDateLabel();

        Uri = uri;
        if (uri is not null)
        {
            Tooltip = uri.ToString();
        }
    }

    public string Title { get; }
    public DateTimeOffset? DueDate { get; }
    public SolidColorBrush TitleForeground => IsEnabled ? _titleDefaultForeground : _titleCompletedForeground;
    public Visibility DueDateVisibility { get; }
    public Uri? Uri { get; }
    public string? Tooltip { get; }

    [ObservableProperty]
    private string dueDateLabel = string.Empty;

    [ObservableProperty]
    private SolidColorBrush dueDateBackground;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForeground))]
    private bool isEnabled = true;

    public DateTime LastRefresh = DateTime.Now;

    public void Complete()
    {
        IsEnabled = false;

        _widget.CompleteTask(Title);
    }

    public void Clicked() => _eventHub.RaiseHyperlinkClicked(Title, Uri!, _widget.Title, DateTime.Now);

    /// <summary>
    /// Sets the due date label text and background based on the due date.
    /// </summary>
    public void SetDueDateLabel()
    {
        if (!DueDate.HasValue)
        {
            return;
        }

        var now = DateTime.Now;
        if (DueDate.Value.Date < now.Date)
        {
            if (DueDate.Value.Date == now.Date.AddDays(-1))
            {
                DueDateLabel = "Yesterday";
            }
            else
            {
                DueDateLabel = DueDate.Value.ToString(DueDateFormat, Global.Culture);
            }

            DueDateBackground = _dueDateOverdueBackground;
        }
        else if (DueDate.Value.Date == now.Date)
        {
            DueDateLabel = "Today";
            DueDateBackground = _dueDateOverdueBackground;
        }
        else
        {
            if (DueDate.Value.Date == now.Date.AddDays(1))
            {
                DueDateLabel = "Tomorrow";
                DueDateBackground = _dueDateAlmostDueBackground;
            }
            else
            {
                if (DueDate.Value.Date < now.Date.AddDays(7))
                {
                    DueDateLabel = DueDate.Value.Date.ToString(DueDateWeekdayFormat, Global.Culture);
                }
                else
                {
                    DueDateLabel = DueDate.Value.ToString(DueDateFormat, Global.Culture);
                }

                DueDateBackground = _dueDateBackground;
            }
        }
    }
}
