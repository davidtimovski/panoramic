using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class EditLinkViewModel : ObservableObject
{
    public EditLinkViewModel(string title, Uri uri)
    {
        Title = title;
        Url = uri.ToString();
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Uri))]
    [NotifyPropertyChangedFor(nameof(NavigationIsEnabled))]
    private string url;

    public Uri? Uri => UriHelper.Create(Url);

    public bool NavigationIsEnabled => Uri is not null;
}
