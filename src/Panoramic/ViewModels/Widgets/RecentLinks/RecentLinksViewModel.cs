﻿using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed partial class RecentLinksViewModel : WidgetViewModel
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly RecentLinksWidget _widget;
    private string searchText = string.Empty;

    public RecentLinksViewModel(
        IStorageService storageService,
        IEventHub eventHub,
        DispatcherQueue dispatcherQueue,
        RecentLinksWidget widget)
    {
        _storageService = storageService;

        _eventHub = eventHub;
        _eventHub.SearchInvoked += SearchInvoked;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _dispatcherQueue = dispatcherQueue;

        _widget = widget;

        Title = widget.Title;

        SetViewModel();
    }

    public string Title { get; }

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = [];

    [ObservableProperty]
    private Visibility filterIconVisibility = Visibility.Collapsed;

    public void ClearRecent()
    {
        _widget.Clear();
        _storageService.EnqueueWidgetWrite(_widget.Id);

        Recent.Clear();
    }

    private void SearchInvoked(object? _, string searchText)
    {
        this.searchText = searchText;
        _dispatcherQueue.TryEnqueue(SetViewModel);
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        _widget.HyperlinkClicked(e.Title, e.Uri, e.Clicked);

        _storageService.EnqueueWidgetWrite(_widget.Id);

        SetViewModel();
    }

    private void SetViewModel()
    {
        var source = _widget.Links.AsEnumerable();
        if (this.searchText.Length > 0)
        {
            source = source.Where(x => x.Matches(this.searchText));

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
        => new(_eventHub, recentLink.Title, recentLink.Uri, recentLink.Clicked);
}
