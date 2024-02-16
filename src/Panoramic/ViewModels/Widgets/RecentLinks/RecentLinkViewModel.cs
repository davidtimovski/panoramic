﻿using System;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed class RecentLinkViewModel(IEventHub eventHub, string title, Uri uri, DateTime clicked)
{
    private readonly IEventHub _eventHub = eventHub;

    public string Title { get; set; } = title;
    public Uri Uri { get; set; } = uri;
    public string Tooltip { get; set; } = uri.Host;
    public DateTime Clicked { get; set; } = clicked;

    public void Click()
    {
        Clicked = DateTime.Now;
        _eventHub.RaiseHyperlinkClicked(Title, Uri, Clicked);
    }
}
