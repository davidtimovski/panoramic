using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Utils.Serialization;

namespace Panoramic.Models.Domain.LinkCollection;

public class LinkCollectionData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public required WidgetType Type { get; init; }

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "My links";

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
