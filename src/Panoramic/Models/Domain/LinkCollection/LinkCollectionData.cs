using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Utils.Serialization;

namespace Panoramic.Models.Domain.LinkCollection;

public sealed class LinkCollectionData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; } = WidgetType.LinkCollection;

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "My links";

    [JsonRequired]
    [JsonPropertyName("searchable")]
    public bool Searchable { get; init; } = true;

    [JsonRequired]
    [JsonPropertyName("links")]
    public required List<LinkCollectionItemData> Links { get; init; }
}

public sealed class LinkCollectionItemData
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
