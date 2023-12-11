using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Panoramic.Models.Domain;

public class RecentLinksWidgetData : WidgetData
{
    public RecentLinksWidgetData()
        : base(WidgetType.RecentLinks)
    {
    }

    [JsonPropertyName("capacity")]
    public required int Capacity { get; set; }

    [JsonPropertyName("onlyFromToday")]
    public required bool OnlyFromToday { get; set; }

    [JsonPropertyName("links")]
    public required List<RecentLink> Links { get; init; }

    public static RecentLinksWidgetData New(Area area) => new()
    {
        Id = Guid.Empty,
        Area = area,
        Title = "Recent",
        Capacity = 15,
        OnlyFromToday = false,
        Links = new()
    };
}

public class RecentLink
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("clicked")]
    public required DateTime Clicked { get; set; }
}
