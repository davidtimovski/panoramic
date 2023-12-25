using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Panoramic.Models.Domain.LinkCollection;

public class LinkCollectionData : WidgetData
{
    [JsonRequired]
    [JsonPropertyName("links")]
    public required List<LinkCollectionItemData> Links { get; init; }
}

public class LinkCollectionItemData
{
    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonRequired]
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }

    [JsonRequired]
    [JsonPropertyName("order")]
    public required short Order { get; init; }
}
