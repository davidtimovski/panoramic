using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed class LinkViewModel(IEventHub eventHub, string drawerName, string title, Uri uri)
{
    public string Title { get; } = title;
    public Uri Uri { get; } = uri;
    public string Tooltip { get; } = uri.Host;

    public event EventHandler<EventArgs>? Clicked;

    public void Click()
    {
        eventHub.RaiseHyperlinkClicked(Title, Uri, drawerName, DateTime.Now);
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
