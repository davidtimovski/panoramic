using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services;
using Panoramic.Services.Search;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed partial class RecentLinksViewModel : WidgetViewModel
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly ISearchService _searchService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly RecentLinksWidget _widget;

    public RecentLinksViewModel(
        IStorageService storageService,
        IEventHub eventHub,
        ISearchService searchService,
        DispatcherQueue dispatcherQueue,
        RecentLinksWidget widget)
    {
        _storageService = storageService;

        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _searchService = searchService;
        if (widget.Searchable)
        {
            _searchService.SearchInvoked += SearchInvoked;
        }

        _dispatcherQueue = dispatcherQueue;

        _widget = widget;

        HeaderBackgroundBrush = ResourceUtil.HighlightBrushes[widget.HeaderHighlight];
        Title = widget.Title;

        SetViewModel();
    }

    public SolidColorBrush HeaderBackgroundBrush { get; }

    public string Title { get; }

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = [];

    [ObservableProperty]
    private Visibility filterIconVisibility = Visibility.Collapsed;

    public void ClearRecent()
    {
        _widget.Clear();
        _storageService.EnqueueWidgetWrite(_widget.Id, "Recent Links cleared");

        Recent.Clear();
    }

    private void SearchInvoked(object? _, EventArgs e) => _dispatcherQueue.TryEnqueue(SetViewModel);

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        _widget.HyperlinkClicked(e.Title, e.Uri, e.Context, e.Clicked);

        _storageService.EnqueueWidgetWrite(_widget.Id, "Hyperlink clicked in Recent Links");

        SetViewModel();
    }

    private void SetViewModel()
    {
        var source = _widget.Links.AsEnumerable();
        if (_searchService.SearchText.Length > 0)
        {
            source = source.Where(x => x.Matches(_searchService.SearchText));

            FilterIconVisibility = Visibility.Visible;
        }
        else
        {
            FilterIconVisibility = Visibility.Collapsed;
        }

        var filteredLinkVms = source.Select(MapToViewModel).ToList();

        Recent.Clear();
        foreach (var recentLinkVm in filteredLinkVms)
        {
            Recent.Add(recentLinkVm);
        }
    }

    private RecentLinkViewModel MapToViewModel(RecentLink recentLink)
        => new(_eventHub, recentLink.Title, recentLink.Uri, recentLink.Context, recentLink.Clicked);
}
