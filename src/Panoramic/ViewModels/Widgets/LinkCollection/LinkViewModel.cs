using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed class LinkViewModel
{
    private readonly IEventHub _eventHub;
    private readonly string _widgetTitle;

    public LinkViewModel(IEventHub eventHub, string widgetTitle, string title, Uri uri)
    {
        _eventHub = eventHub;
        _widgetTitle = widgetTitle;

        Title = title;
        Uri = uri;
        Tooltip = uri.ToString();
    }

    public string Title { get; }
    public Uri Uri { get; }
    public string Tooltip { get; }

    public void Click() => _eventHub.RaiseHyperlinkClicked(Title, Uri, _widgetTitle, DateTime.Now);
}
