using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksViewModel : ObservableObject
{
    private readonly string _section;
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly RecentLinksWidgetData _data;

    public RecentLinksViewModel(
        string section,
        IStorageService storageService,
        IEventHub eventHub,
        RecentLinksWidgetData data)
    {
        _storageService = storageService;
        _section = section;

        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _data = data;

        Title = data.Title;

        SetViewModel();
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = new();

    public void ClearRecent()
    {
        _data.Links.Clear();
        _storageService.EnqueueSectionWrite(_section);

        Recent.Clear();        
    }

    private void HyperlinkClicked(object? _, HyperlinkClickedEventArgs e)
    {
        var url = e.Uri.ToString();

        var link = _data.Links.FirstOrDefault(x => string.Equals(x.Url, url, StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            _data.Links.Add(new RecentLink { Title = e.Title, Url = e.Uri.ToString(), Clicked = e.Clicked });

            if (_data.Links.Count > _data.Capacity)
            {
                _data.Links.RemoveAt(0);
            }
        }
        else
        {
            link.Clicked = e.Clicked;
        }

        var query = _data.Links.AsEnumerable();
        if (_data.OnlyFromToday)
        {
            query = query.Where(x => x.Clicked >= DateTime.Today);
        }

        _data.Links = query.OrderByDescending(x => x.Clicked).Take(_data.Capacity).ToList();
        _storageService.EnqueueSectionWrite(_section);

        SetViewModel();
    }

    private void SetViewModel()
    {
        Recent.Clear();

        foreach (var recentLink in _data.Links)
        {
            Recent.Add(new RecentLinkViewModel(_eventHub, recentLink.Title, new Uri(recentLink.Url, UriKind.Absolute), recentLink.Clicked));
        }
    }
}
