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

    private string url = string.Empty;
    public string Url
    {
        get => url;
        set
        {
            if (SetProperty(ref url, value))
            {
                OnPropertyChanged();
                ValidateAndEmit();
            }
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public bool CanBeCreated() => Title.Trim().Length > 0
        && widget.TaskCanBeCreated(Title.Trim())
        && (Url.Trim().Length == 0 || Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _));

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
}
