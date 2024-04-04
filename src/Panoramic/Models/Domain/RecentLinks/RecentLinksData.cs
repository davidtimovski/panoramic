using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Utils.Serialization;

namespace Panoramic.Models.Domain.RecentLinks;

public sealed class RecentLinksData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; } = WidgetType.RecentLinks;

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "Recent";

    [JsonRequired]
    [JsonPropertyName("capacity")]
    public int Capacity { get; init; } = 15;

    [JsonRequired]
    [JsonPropertyName("onlyFromToday")]
    public bool OnlyFromToday { get; init; }

    [JsonRequired]
    [JsonPropertyName("searchable")]
    public bool Searchable { get; init; } = true;

    [JsonRequired]
    [JsonPropertyName("links")]
    public required List<RecentLinkData> Links { get; init; }
}

public sealed class RecentLinkData
{
    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonRequired]
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }

    [JsonRequired]
    [JsonPropertyName("clicked")]
    public required DateTime Clicked { get; init; }
}
