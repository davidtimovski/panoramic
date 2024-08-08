using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed class LinkViewModel
{
    private readonly IEventHub _eventHub;
    private readonly string _drawerName;

    public LinkViewModel(IEventHub eventHub, string drawerName, string title, Uri uri)
    {
        _eventHub = eventHub;
        _drawerName = drawerName;

        Title = title;
        Uri = uri;
        Tooltip = uri.ToString();
    }

    public string Title { get; }
    public Uri Uri { get; }
    public string Tooltip { get; }

    public event EventHandler<EventArgs>? Clicked;

    public void Click()
    {
        _eventHub.RaiseHyperlinkClicked(Title, Uri, _drawerName, DateTime.Now);
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
