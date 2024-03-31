using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class NewTaskViewModel(ChecklistWidget widget) : ObservableObject
{
    private string title = string.Empty;
    public string Title
    {
        get => title;
        set
        {
            if (SetProperty(ref title, value))
            {
                OnPropertyChanged();
                ValidateAndEmit();
            }
        }
    }

    [ObservableProperty]
    private DateTimeOffset? dueDate;

    public event EventHandler<ValidationEventArgs>? Validated;

    public bool CanBeCreated() => Title.Trim().Length > 0 && widget.TaskCanBeCreated(Title.Trim());

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
}
