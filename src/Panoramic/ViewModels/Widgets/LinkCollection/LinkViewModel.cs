using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed class LinkViewModel(IEventHub eventHub, string widgetTitle, string title, Uri uri)
{
    public string Title { get; } = title;
    public Uri Uri { get; } = uri;
    public string Tooltip { get; } = uri.Host;

    public void Click() => eventHub.RaiseHyperlinkClicked(Title, Uri, widgetTitle, DateTime.Now);
}
