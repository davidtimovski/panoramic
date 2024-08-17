using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Panoramic.Data;
using Panoramic.Services;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class ViewDrawerViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly LinkDrawerData _data;

    public ViewDrawerViewModel(IEventHub eventHub, LinkDrawerData data)
    {
        _eventHub = eventHub;
        _data = data;

        Name = _data.Name;

        SetLinks(string.Empty);
    }

    public event EventHandler<EventArgs>? LinkClicked;

    public string Name { get; }

    [ObservableProperty]
    private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => SetLinks(value);

    public GridLength LinksListViewHeight => CalculateLinksListViewHeight();

    public ObservableCollection<LinkViewModel> Links { get; } = [];

    private void SetLinks(string searchText)
    {
        var source = _data.Links.AsEnumerable();

        var trimmed = searchText.Trim();
        if (trimmed.Length > 0)
        {
            source = source.Select(x => x.Matches(trimmed, Name))
                .Where(x => x.Weight > 0)
                .OrderByDescending(x => x.Weight)
                .Select(x => x.Result);
        }

        var filteredLinkVms = source.Select(MapToLinkViewModel).ToList();

        Links.Clear();
        foreach (var linkVm in filteredLinkVms)
        {
            Links.Add(linkVm);
        }
    }

    private LinkViewModel MapToLinkViewModel(LinkDrawerLinkData link)
    {
        var searchTermsString = string.Join(LinkDrawerLinkData.SearchTermsSeparator, link.SearchTerms);
        var vm = new LinkViewModel(_eventHub, _data.Name, link.Title, link.Uri);
        vm.Clicked += (object? _, EventArgs e) => { LinkClicked?.Invoke(this, EventArgs.Empty); };

        return vm;
    }

    private GridLength CalculateLinksListViewHeight()
    {
        if (Links.Count < 4)
        {
            return new GridLength(120);
        }

        if (Links.Count < 15)
        {
            return GridLength.Auto;
        }

        return new GridLength(500);
    }
}
