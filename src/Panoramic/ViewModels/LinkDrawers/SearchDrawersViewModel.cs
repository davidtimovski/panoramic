using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Services;
using Panoramic.Services.Drawers;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class SearchDrawersViewModel : ObservableObject
{
    private readonly IDrawerService _drawerService;
    private readonly IEventHub _eventHub;

    public SearchDrawersViewModel(IDrawerService drawerService, IEventHub eventHub)
    {
        _drawerService = drawerService;
        _eventHub = eventHub;

        SetLinks(string.Empty);
    }

    public event EventHandler<EventArgs>? NavigatedToLink;

    [ObservableProperty]
    private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => SetLinks(value);

    public ObservableCollection<SearchedLinkViewModel> Links { get; } = [];

    private void SetLinks(string searchText)
    {
        Links.Clear();

        var trimmed = searchText.Trim();
        if (trimmed.Length == 0)
        {
            return;
        }

        var matchedLinks = _drawerService.SearchDrawers(searchText);

        var matchedLinkVms = matchedLinks.Select(x => MapToLinkViewModel(x.Result, x.DrawerName)).ToList();

        foreach (var linkVm in matchedLinkVms)
        {
            Links.Add(linkVm);
        }
    }

    private SearchedLinkViewModel MapToLinkViewModel(LinkDrawerLinkData link, string drawerName)
    {
        var searchTermsString = string.Join(LinkDrawerLinkData.SearchTermsSeparator, link.SearchTerms);
        var vm = new SearchedLinkViewModel(_eventHub, drawerName, link.Title, link.Uri);
        vm.Clicked += (object? _, EventArgs e) => { NavigatedToLink?.Invoke(this, EventArgs.Empty); };

        return vm;
    }
}
