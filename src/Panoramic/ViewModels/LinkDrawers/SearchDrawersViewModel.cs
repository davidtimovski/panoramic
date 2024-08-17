using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Panoramic.Data;
using Panoramic.Services;
using Panoramic.Services.Drawers;
using Panoramic.Utils;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class SearchDrawersViewModel : ObservableObject
{
    private readonly IDrawerService _drawerService;
    private readonly IEventHub _eventHub;
    private readonly SolidColorBrush _linkBackgroundBrush;
    private readonly SolidColorBrush _selectedLinkBorderBrush;
    private int selectedIndex;

    public SearchDrawersViewModel(IDrawerService drawerService, IEventHub eventHub, Page page)
    {
        _drawerService = drawerService;
        _eventHub = eventHub;
        _linkBackgroundBrush = ResourceUtil.GetBrushFromPage("LinkBorderBrush", page);
        _selectedLinkBorderBrush = ResourceUtil.GetBrushFromPage("SelectedLinkBorderBrush", page);

        SetLinks(string.Empty);
    }

    public event EventHandler<EventArgs>? NavigatedToLink;

    [ObservableProperty]
    private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => SetLinks(value);

    public ObservableCollection<SearchedLinkViewModel> Links { get; } = [];

    public void SelectNextLink()
    {
        if (selectedIndex == Links.Count - 1)
        {
            return;
        }

        var currentlySelectedLink = Links[selectedIndex];
        currentlySelectedLink.Selected = false;

        selectedIndex++;

        Links[selectedIndex].Selected = true;
    }

    public void SelectPreviousLink()
    {
        if (selectedIndex == 0)
        {
            return;
        }

        var currentlySelectedLink = Links[selectedIndex];
        currentlySelectedLink.Selected = false;

        selectedIndex--;

        Links[selectedIndex].Selected = true;
    }

    public void NavigateToCurrentLink() => Links[selectedIndex].Click();

    private void SetLinks(string searchText)
    {
        Links.Clear();

        var trimmed = searchText.Trim();
        if (trimmed.Length == 0)
        {
            return;
        }

        var matchedLinks = _drawerService.SearchDrawers(searchText);

        var matchedLinkVms = matchedLinks.Select((x, i) => MapToLinkViewModel(x.Result, x.DrawerName, selected: i == 0)).ToList();

        if (matchedLinkVms.Count == 0)
        {
            return;
        }

        foreach (var linkVm in matchedLinkVms)
        {
            Links.Add(linkVm);
        }

        selectedIndex = 0;
    }

    private SearchedLinkViewModel MapToLinkViewModel(LinkDrawerLinkData link, string drawerName, bool selected)
    {
        var searchTermsString = string.Join(LinkDrawerLinkData.SearchTermsSeparator, link.SearchTerms);
        var vm = new SearchedLinkViewModel(_eventHub, drawerName, _linkBackgroundBrush, _selectedLinkBorderBrush, link.Title, link.Uri, selected);
        vm.Clicked += (object? _, EventArgs e) => { NavigatedToLink?.Invoke(this, EventArgs.Empty); };

        return vm;
    }
}
