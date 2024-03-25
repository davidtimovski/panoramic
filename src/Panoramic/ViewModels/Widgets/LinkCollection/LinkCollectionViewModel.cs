using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Dispatching;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed class LinkCollectionViewModel : WidgetViewModel
{
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly LinkCollectionWidget _widget;

    public LinkCollectionViewModel(IEventHub eventHub, DispatcherQueue dispatcherQueue, LinkCollectionWidget widget)
    {
        _eventHub = eventHub;
        _eventHub.SearchInvoked += SearchInvoked;

        _dispatcherQueue = dispatcherQueue;

        _widget = widget;

        Title = widget.Title;

        foreach (var item in _widget.Links)
        {
            Links.Add(MapToViewModel(item));
        }
    }

    public string Title { get; }

    public ObservableCollection<LinkViewModel> Links = [];

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
            Links.Clear();
            foreach (var recentLinkVm in filteredLinkVms)
            {
                Links.Add(recentLinkVm);
            }
        });
    }

    private LinkViewModel MapToViewModel(LinkCollectionItem item)
        => new(_eventHub, Title, item.Title, item.Uri);
}
