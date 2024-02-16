using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionViewModel : ObservableObject
{
    public LinkCollectionViewModel(IEventHub eventHub, LinkCollectionWidget widget)
    {
        Title = widget.Title;

        foreach (var item in widget.Links)
        {
            Links.Add(new LinkViewModel(eventHub, item.Title, item.Uri));
        }
    }

    [ObservableProperty]
    private string title;

    public ObservableCollection<LinkViewModel> Links = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public SolidColorBrush Background => Highlighted
        ? (Application.Current.Resources["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!
        : (Application.Current.Resources["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
}
