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

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private DateTimeOffset? dueDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Uri))]
    [NotifyPropertyChangedFor(nameof(NavigationIsEnabled))]
    private string url;

    public Uri? Uri => Url.Trim().Length > 0 && Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var createdUri) ? createdUri : null;

    public DateTime Created { get; }

    public bool NavigationIsEnabled => Url.Trim().Length > 0 && Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _);
}
