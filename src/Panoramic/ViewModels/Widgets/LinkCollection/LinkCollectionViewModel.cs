using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;
using Panoramic.Services.Search;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionViewModel : WidgetViewModel
{
    private readonly IEventHub _eventHub;
    private readonly ISearchService _searchService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly LinkCollectionWidget _widget;

    public LinkCollectionViewModel(IEventHub eventHub, ISearchService searchService, DispatcherQueue dispatcherQueue, LinkCollectionWidget widget)
    {
        _eventHub = eventHub;

        _searchService = searchService;
        if (widget.Searchable)
        {
            _searchService.SearchInvoked += SearchInvoked;
        }

        _dispatcherQueue = dispatcherQueue;

        _widget = widget;

        HeaderBackgroundBrush = ResourceUtil.WidgetHeaderBrushes[widget.HeaderHighlight];
        Title = widget.Title;

        SetViewModel();
    }

    public SolidColorBrush HeaderBackgroundBrush { get; }

    public string Title { get; }

    public readonly ObservableCollection<LinkViewModel> Links = [];

    [ObservableProperty]
    private Visibility filterIconVisibility = Visibility.Collapsed;

    private void SearchInvoked(object? _, EventArgs e) => _dispatcherQueue.TryEnqueue(SetViewModel);

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

        Links.Clear();
        foreach (var linkVm in filteredLinkVms)
        {
            Links.Add(linkVm);
        }
    }

    private LinkViewModel MapToViewModel(LinkCollectionItem item)
        => new(_eventHub, Title, item.Title, item.Uri);
}
