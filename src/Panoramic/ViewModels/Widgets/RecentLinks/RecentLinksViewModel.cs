using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksViewModel : ObservableObject
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly RecentLinksWidget _widget;

    public RecentLinksViewModel(
        IStorageService storageService,
        IEventHub eventHub,
        RecentLinksWidget widget)
    {
        _storageService = storageService;

        _eventHub = eventHub;
        _eventHub.HyperlinkClicked += HyperlinkClicked;

        _widget = widget;

        Title = widget.Title;

        SetViewModel();
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<RecentLinkViewModel> Recent { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public SolidColorBrush Background => Highlighted
        ? (Application.Current.Resources["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!
        : (Application.Current.Resources["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;

    public void ClearRecent()
    {
        _widget.Clear();
        _storageService.EnqueueWidgetWrite(_widget.Id);

        Recent.Clear();
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
            Recent.Add(new RecentLinkViewModel(_eventHub, recentLink.Title, recentLink.Uri, recentLink.Clicked));
        }
    }
}
