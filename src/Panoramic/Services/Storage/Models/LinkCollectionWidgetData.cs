using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Models;

namespace Panoramic.Services.Storage.Models;

public class LinkCollectionWidgetData : WidgetData
{
    public LinkCollectionWidgetData()
        : base(WidgetType.LinkCollection)
    {
    }

    [JsonPropertyName("links")]
    public required List<LinkCollectionItem> Links { get; init; }

    public static LinkCollectionWidgetData New(Area area) => new()
    {
        Id = Guid.Empty,
        Area = area,
        Title = "Links",
        Links = new()
    };
}

public class LinkCollectionItem
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("clicked")]
    public required List<DateTime> Clicks { get; init; } = new();
}
