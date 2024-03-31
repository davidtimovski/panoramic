using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class EditTaskViewModel : ObservableObject
{
    public EditTaskViewModel(string title, DateTimeOffset? dueDate, DateTime created)
    {
        Title = title;
        DueDate = dueDate;
        Created = created;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private DateTimeOffset? dueDate;

    public DateTime Created { get; }
}
