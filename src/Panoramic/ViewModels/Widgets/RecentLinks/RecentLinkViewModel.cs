using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public class RecentLinkViewModel
{
    private readonly IEventHub _eventHub;

    public RecentLinkViewModel(IEventHub eventHub, string title, Uri uri, DateTime clicked)
    {
        _eventHub = eventHub;

        Title = title;
        Uri = uri;
        Clicked = clicked;
    }

    public string Title { get; set; }
    public Uri Uri { get; set; }
    public DateTime Clicked { get; set; }

    public void Click()
    {
        Clicked = DateTime.Now;
        _eventHub.RaiseHyperlinkClicked(Title, Uri, Clicked);
    }
}
