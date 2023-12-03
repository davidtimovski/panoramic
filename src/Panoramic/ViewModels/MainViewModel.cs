using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Panoramic.Services;

namespace Panoramic.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;

    public MainViewModel(IEventHub eventHub, DispatcherQueue dispatcherQueue)
    {
        _eventHub = eventHub;
        _dispatcherQueue = dispatcherQueue;

        _eventHub.HyperlinkClicked += HyperlinkClicked;

        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel", new Uri("https://www.google.com/search?q=1", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 2", new Uri("https://www.google.com/search?q=2", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 3", new Uri("https://www.google.com/search?q=3", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 4", new Uri("https://www.google.com/search?q=4", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 5", new Uri("https://www.google.com/search?q=5", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 6", new Uri("https://www.google.com/search?q=6", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 7", new Uri("https://www.google.com/search?q=7", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 8", new Uri("https://www.google.com/search?q=8", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 9", new Uri("https://www.google.com/search?q=9", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 10", new Uri("https://www.google.com/search?q=10", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 11", new Uri("https://www.google.com/search?q=11", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 12", new Uri("https://www.google.com/search?q=12", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
        Bookmarks.Add(new BookmarkViewModel(_eventHub, "Pull requests lel 13", new Uri("https://www.google.com/search?q=13", UriKind.Absolute), new List<DateTime> { DateTime.Now }));
    }

    public ObservableCollection<BookmarkViewModel> Bookmarks = new();

    public void AddBookmark(string title, string uri)
    {
        ReorderBookmarks(new BookmarkViewModel(_eventHub, title, new Uri(uri, UriKind.Absolute), new List<DateTime> { DateTime.Now }));
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        ReorderBookmarks();
    }

    private void ReorderBookmarks(BookmarkViewModel? bookmarkToAdd = null)
    {
        var bookmarksReordered = Bookmarks.OrderByDescending(x => x.Weight).ThenByDescending(x => x.LastClick).ToList();

        _dispatcherQueue.TryEnqueue(() =>
        {
            Bookmarks.Clear();

            if (bookmarkToAdd is not null)
            {
                Bookmarks.Add(bookmarkToAdd);
            }

            foreach (var bookmark in bookmarksReordered)
            {
                Bookmarks.Add(bookmark);
            }
        });
    }
}
