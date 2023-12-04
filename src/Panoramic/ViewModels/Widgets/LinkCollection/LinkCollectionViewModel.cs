using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

// TODO: Add possibility of removing links
// Ordering links
// Or have a setting for ordered or auto-ordered links
public partial class LinkCollectionViewModel : ObservableObject
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly LinkCollectionWidgetData _data;

    public LinkCollectionViewModel(
        IStorageService storageService,
        IEventHub eventHub,
        DispatcherQueue dispatcherQueue,
        LinkCollectionWidgetData data)
    {
        _storageService = storageService;

        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _dispatcherQueue = dispatcherQueue;
        _data = data;

        Title = data.Title;

        foreach (var item in data.Links)
        {
            Links.Add(new LinkViewModel(eventHub, item.Title, new Uri(item.Url, UriKind.Absolute), item.Clicks));
        }
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<LinkViewModel> Links = new();

    public void AddLink(string title, string url)
    {
        var now = DateTime.Now;

        _data.Links.Add(new LinkCollectionItem { Title = title, Url = url, Clicks = new List<DateTime> { now } });

        ReorderBookmarks(new LinkViewModel(_eventHub, title, new Uri(url, UriKind.Absolute), new List<DateTime> { now }));
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        ReorderBookmarks();
    }

    private void ReorderBookmarks(LinkViewModel? bookmarkToAdd = null)
    {
        var bookmarksReordered = Links.OrderByDescending(x => x.Weight).ThenByDescending(x => x.LastClick).ToList();

        _dispatcherQueue.TryEnqueue(() =>
        {
            Links.Clear();

            if (bookmarkToAdd is not null)
            {
                Links.Add(bookmarkToAdd);
            }

            foreach (var bookmark in bookmarksReordered)
            {
                Links.Add(bookmark);
            }
        });
    }
}
