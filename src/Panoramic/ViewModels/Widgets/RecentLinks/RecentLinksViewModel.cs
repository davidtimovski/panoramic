using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Dispatching;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed partial class RecentLinksViewModel : WidgetViewModel
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly RecentLinksWidget _widget;

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

    public void ClearRecent()
    {
        _widget.Clear();
        _storageService.EnqueueWidgetWrite(_widget.Id);

        Recent.Clear();
    }

    private void SearchInvoked(object? _, string searchText)
    {
        var source = _widget.Links.AsEnumerable();
        if (searchText.Length > 0)
        {
            source = source.Where(x => x.Matches(searchText));
        }

        var filteredLinkVms = source.Select(MapToViewModel).ToList();

        _dispatcherQueue.TryEnqueue(() =>
        {
            Recent.Clear();
            foreach (var recentLinkVm in filteredLinkVms)
            {
                Recent.Add(recentLinkVm);
            }
        });
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        _widget.HyperlinkClicked(e.Title, e.Uri, e.Clicked);

        _storageService.EnqueueWidgetWrite(_widget.Id);

        SetViewModel();
    }

    private void SetViewModel()
    {
        Recent.Clear();

        foreach (var recentLink in _widget.Links)
        {
            Recent.Add(MapToViewModel(recentLink));
        }
    }

    private RecentLinkViewModel MapToViewModel(RecentLink recentLink)
        => new(_eventHub, recentLink.Title, recentLink.Uri, recentLink.Clicked);
}
