using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Panoramic.Models.Domain.LinkCollection;

public class LinkCollectionWidget : Widget
{
    public const string DefaultTitle = "My links";

    /// <summary>
    /// Constructs a new link collection widget.
    /// </summary>
    public LinkCollectionWidget(Area area, string title)
        : base(Guid.NewGuid(), WidgetType.LinkCollection, area, title)
    {
        links = new();
    }

    /// <summary>
    /// Constructs a link collection widget based on en existing one.
    /// </summary>
    public LinkCollectionWidget(LinkCollectionData data)
        : base(data.Id, WidgetType.LinkCollection, data.Area, data.Title)
    {
        links = data.Links.Select(x => new LinkCollectionItem { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList();
    }

    private List<LinkCollectionItem> links;
    public IReadOnlyList<LinkCollectionItem> Links
    {
        get
        {
            return links.OrderBy(x => x.Order).ToList();
        }
        private set
        {
            links = value.ToList();
        }
    }

    public void SetData(List<LinkCollectionItem> links)
    {
        this.links = links;
    }

    public LinkCollectionData GetData() =>
        new()
        {
            Id = Id,
            Type = WidgetType.LinkCollection,
            Area = Area,
            Title = Title,
            Links = links.Select(x => new LinkCollectionItemData { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList()
        };

    public string Serialize(JsonSerializerOptions options)
    {
        var data = GetData();
        return JsonSerializer.Serialize(data, options);
    }

    public static LinkCollectionWidget Load(string json, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<LinkCollectionData>(json, options)!;
        return new(data);
    }
}

public class LinkCollectionItem
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }
}
