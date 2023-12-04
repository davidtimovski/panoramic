using System;
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
    private readonly int _capacity;
    private readonly bool _resetEveryDay; // TODO: Implement

    public RecentLinksViewModel(IEventHub eventHub, RecentLinksWidgetData data)
    {
        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _data = data;

        _capacity = data.Capacity;
        _resetEveryDay = data.ResetEveryDay;
        Title = data.Title;

        SetViewModel();
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = new();

    public void ClearRecent()
    {
        Recent.Clear();
    }

    // TODO: Is ordering guaranteed?
    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        var url = e.Uri.ToString();

        var link = _data.Links.FirstOrDefault(x => string.Equals(x.Url, url, StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            _data.Links.Add(new RecentLink { Title = e.Title, Url = e.Uri.ToString(), Clicked = e.Clicked });

            if (_data.Links.Count > _capacity)
            {
                _data.Links.RemoveAt(0);
            }
        }
        else
        {
            link.Clicked = e.Clicked;
        }

        SetViewModel();
    }

    private void SetViewModel()
    {
        Recent.Clear();

        var ordered = _data.Links.OrderByDescending(x => x.Clicked).Take(_capacity).ToList();
        foreach (var recentLink in ordered)
        {
            Recent.Add(new RecentLinkViewModel(_eventHub, recentLink.Title, new Uri(recentLink.Url, UriKind.Absolute), recentLink.Clicked));
        }
    }
}
