using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed class LinkViewModel(IEventHub eventHub, string title, Uri uri)
{
    private readonly IEventHub _eventHub = eventHub;

    public string Title { get; set; } = title;
    public Uri Uri { get; set; } = uri;
    public string Tooltip { get; set; } = uri.Host;
    public short Order { get; set; }

    public void Clicked()
    {
        _eventHub.RaiseHyperlinkClicked(Title, Uri, DateTime.Now);
    }
}
