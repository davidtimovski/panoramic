using System;
using Panoramic.Services;
using Windows.System;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed class SearchedLinkViewModel
{
    private readonly IEventHub _eventHub;
    private readonly string _drawerName;

    public SearchedLinkViewModel(IEventHub eventHub, string drawerName, string title, Uri uri)
    {
        _eventHub = eventHub;
        _drawerName = drawerName;

        Title = title;
        Uri = uri;
        DrawerName = drawerName;
    }

    public string Title { get; }
    public Uri Uri { get; }
    public string DrawerName { get; }

    public event EventHandler<EventArgs>? Clicked;

    public async void Click()
    {
        await Launcher.LaunchUriAsync(Uri);

        _eventHub.RaiseHyperlinkClicked(Title, Uri, _drawerName, DateTime.Now);
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
