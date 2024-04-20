using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class EditTaskViewModel : ObservableObject
{
    public EditTaskViewModel(string title, DateTimeOffset? dueDate, Uri? uri, DateTime created)
    {
        Title = title;
        DueDate = dueDate;
        Url = uri is not null ? uri.ToString() : string.Empty;
        Created = created;
    }

    public event EventHandler<EventArgs>? Updated;

    private string title = string.Empty;
    public string Title
    {
        get => title;
        set
        {
            if (SetProperty(ref title, value))
            {
                OnPropertyChanged();
                Updated?.Invoke(this, EventArgs.Empty);
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

                Uri = Url.Trim().Length > 0 && Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var createdUri) ? createdUri : null;
                NavigationIsEnabled = Uri is not null;

                Updated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public DateTime Created { get; }

    [ObservableProperty]
    private Uri? uri;

    [ObservableProperty]
    private bool navigationIsEnabled;

    public bool IsValid() => Title.Trim().Length > 0
        && (Url.Trim().Length == 0 || Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _));
}
