using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed class RecentLinkViewModel(IEventHub eventHub, string title, Uri uri, DateTime clicked)
{
    public string Title { get; } = title;
    public Uri Uri { get; } = uri;
    public string Tooltip { get; } = uri.Host;

    public void Click()
    {
        clicked = DateTime.Now;
        eventHub.RaiseHyperlinkClicked(Title, Uri, clicked);
    }
}
