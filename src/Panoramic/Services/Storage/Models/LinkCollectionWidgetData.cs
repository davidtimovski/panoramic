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
    public List<LinkCollectionItem> Links { get; set; } = new();
}

public class LinkCollectionItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("clicked")]
    public List<DateTime> Clicks { get; set; } = new();
}
