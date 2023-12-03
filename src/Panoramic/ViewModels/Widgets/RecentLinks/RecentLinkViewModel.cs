using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public class RecentLinkViewModel
{
    private readonly IEventHub _eventHub;

    public RecentLinkViewModel(IEventHub eventHub, string title, Uri uri)
    {
        _eventHub = eventHub;

        Id = uri.ToString();
        Title = title;
        Uri = uri;
    }

    public string Id { get; set; }
    public string Title { get; set; }
    public Uri Uri { get; set; }
    public DateTime LastClick { get; set; } = DateTime.Now;

    public void Clicked()
    {
        LastClick = DateTime.Now;
        _eventHub.RaiseHyperlinkClicked(Id, Title, Uri);
    }
}
