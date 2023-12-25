using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Panoramic.Models.Domain.RecentLinks;

public class RecentLinksData : WidgetData
{
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
