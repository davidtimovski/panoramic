using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly int _capacity;
    private readonly bool _resetEveryDay; // TODO

    public RecentLinksViewModel(IEventHub eventHub, RecentLinksWidgetData data)
    {
        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _capacity = data.Capacity;
        _resetEveryDay = data.ResetEveryDay;
        Title = data.Title;

        foreach (var recentLink in data.Links)
        {
            Recent.Add(new RecentLinkViewModel(eventHub, recentLink.Title, new Uri(recentLink.Url, UriKind.Absolute), recentLink.Clicked));
        }
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = new();

    public void ClearRecent()
    {
        Recent.Clear();
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        List<RecentLinkViewModel> recentLinks;

        var recent = Recent.FirstOrDefault(x => x.Id == e.Id);
        if (recent is null)
        {
            var viewModels = Recent.ToList();
            viewModels.Add(new RecentLinkViewModel(_eventHub, e.Title, e.Uri, e.Clicked));

            recentLinks = viewModels.OrderByDescending(x => x.Clicked).Take(_capacity).ToList();
        }
        else
        {
            recent.Clicked = DateTime.Now;
            recentLinks = Recent.OrderByDescending(x => x.Clicked).Take(_capacity).ToList();
        }

        Recent.Clear();

        foreach (var recentLink in recentLinks)
        {
            Recent.Add(recentLink);
        }
    }
}
