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
    public required WidgetType Type { get; init; }

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonRequired]
    [JsonPropertyName("capacity")]
    public required int Capacity { get; set; }

    [JsonRequired]
    [JsonPropertyName("onlyFromToday")]
    public required bool OnlyFromToday { get; set; }

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
