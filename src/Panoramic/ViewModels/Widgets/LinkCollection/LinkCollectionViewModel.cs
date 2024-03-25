using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionViewModel : WidgetViewModel
{
    private readonly IEventHub _eventHub;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly LinkCollectionWidget _widget;
    private string searchText = string.Empty;

    public LinkCollectionViewModel(IEventHub eventHub, DispatcherQueue dispatcherQueue, LinkCollectionWidget widget)
    {
        _eventHub = eventHub;
        _eventHub.SearchInvoked += SearchInvoked;

        _dispatcherQueue = dispatcherQueue;

        _widget = widget;

        Title = widget.Title;

        SetViewModel();
    }

    public string Title { get; }

    public ObservableCollection<LinkViewModel> Links = [];

    [ObservableProperty]
    private Visibility filterIconVisibility = Visibility.Collapsed;

    private void SearchInvoked(object? _, string searchText)
    {
        this.searchText = searchText;
        _dispatcherQueue.TryEnqueue(SetViewModel);
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

        Links.Clear();
        foreach (var linkVm in filteredLinkVms)
        {
            Links.Add(linkVm);
        }
    }

    private LinkViewModel MapToViewModel(LinkCollectionItem item)
        => new(_eventHub, Title, item.Title, item.Uri);
}
