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
    private readonly RecentLinksWidgetData _data;

    public RecentLinksViewModel(IEventHub eventHub, RecentLinksWidgetData data)
    {
        _eventHub = eventHub;
        _data = data;

        _eventHub.HyperlinkClicked += HyperlinkClicked;

        Title = data.Title;

        foreach (var recentLink in data.Links)
        {
            Recent.Add(new RecentLinkViewModel(eventHub, recentLink.Title, new Uri(recentLink.Url, UriKind.Absolute)));
        }
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = new();

    public void ClearRecent()
    {
        Recent.Clear();

        _data.Links.Clear();
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        List<RecentLinkViewModel> recentLinks;

        var recent = Recent.FirstOrDefault(x => x.Id == e.Id);
        if (recent is null)
        {
            var viewModels = Recent.ToList();
            viewModels.Add(new RecentLinkViewModel(_eventHub, e.Title, e.Uri));

            recentLinks = viewModels.OrderByDescending(x => x.LastClick).ToList();
        }
        else
        {
            recent.LastClick = DateTime.Now;
            recentLinks = Recent.OrderByDescending(x => x.LastClick).ToList();
        }

        Recent.Clear();

        foreach (var recentLink in recentLinks)
        {
            Recent.Add(recentLink);
        }

        _data.Links = Recent.Select(x => new RecentLink { Title = x.Title, Url = x.Uri.ToString() }).ToList();
    }
}
