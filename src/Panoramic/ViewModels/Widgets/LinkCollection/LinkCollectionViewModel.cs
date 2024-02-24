using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionViewModel : WidgetViewModel
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
}
