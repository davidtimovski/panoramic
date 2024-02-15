using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Utils.Serialization;

namespace Panoramic.Models.Domain.RecentLinks;

public class RecentLinksData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; } = WidgetType.RecentLinks;

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Recent";

    [JsonRequired]
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; } = 15;

    [JsonRequired]
    [JsonPropertyName("onlyFromToday")]
    public bool OnlyFromToday { get; set; } = false;

    [JsonRequired]
    [JsonPropertyName("links")]
    public required List<RecentLinkData> Links { get; init; }
}

public class RecentLinkData
{
    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonRequired]
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }

    [JsonRequired]
    [JsonPropertyName("clicked")]
    public required DateTime Clicked { get; set; }
}
