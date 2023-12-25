using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public class LinkViewModel
{
    private readonly IEventHub _eventHub;

    public LinkViewModel(IEventHub eventHub, string title, Uri uri)
    {
        _eventHub = eventHub;

        Title = title;
        Uri = uri;
    }

    public string Title { get; set; }
    public Uri Uri { get; set; }
    public short Order { get; set; }

    public void Clicked()
    {
        _eventHub.RaiseHyperlinkClicked(Title, Uri, DateTime.Now);
    }
}
