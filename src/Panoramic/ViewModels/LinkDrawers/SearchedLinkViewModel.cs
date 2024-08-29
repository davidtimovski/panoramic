using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Panoramic.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class SearchedLinkViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly string _drawerName;
    private readonly SolidColorBrush _linkBackgroundBrush;
    private readonly SolidColorBrush _selectedLinkBorderBrush;

    public SearchedLinkViewModel(
        IEventHub eventHub,
        string drawerName,
        SolidColorBrush linkBackgroundBrush,
        SolidColorBrush selectedLinkBorderBrush,
        string title,
        Uri uri,
        bool selected)
    {
        _eventHub = eventHub;
        _drawerName = drawerName;
        _linkBackgroundBrush = linkBackgroundBrush;
        _selectedLinkBorderBrush = selectedLinkBorderBrush;

        Title = title;
        Uri = uri;
        DrawerName = drawerName;
        Selected = selected;
    }

    public string Title { get; }
    public Uri Uri { get; }
    public string DrawerName { get; }

    public event EventHandler<EventArgs>? Clicked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    private bool selected;

    public SolidColorBrush BorderBrush => Selected ? _selectedLinkBorderBrush : _linkBackgroundBrush;

    public async void Click()
    {
        await Launcher.LaunchUriAsync(Uri);

        _eventHub.RaiseHyperlinkClicked(Title, Uri, _drawerName, DateTime.Now);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void Copy()
    {
        var package = new DataPackage();
        package.SetText(Uri.ToString());
        Clipboard.SetContent(package);
    }
}
