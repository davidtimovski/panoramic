using System.Collections.ObjectModel;
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
            Links.Add(new LinkViewModel(eventHub, Title, item.Title, item.Uri));
        }
    }

    public string Title { get; }

    public ObservableCollection<LinkViewModel> Links = [];
}
