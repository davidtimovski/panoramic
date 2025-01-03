﻿using System;
using Panoramic.Services;
using Windows.ApplicationModel.DataTransfer;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed class RecentLinkViewModel
{
    private readonly IEventHub _eventHub;

    public RecentLinkViewModel(IEventHub eventHub, string title, Uri uri, string context)
    {
        _eventHub = eventHub;

        Title = title;
        Uri = uri;
        Context = context;
        Tooltip = uri.ToString();
    }

    public string Title { get; }
    public Uri Uri { get; }
    public string Context { get; }
    public string Tooltip { get; }

    public void Click() => _eventHub.RaiseHyperlinkClicked(Title, Uri, Context, DateTime.Now);

    public void Copy()
    {
        var package = new DataPackage();
        package.SetText(Uri.ToString());
        Clipboard.SetContent(package);
    }
}
