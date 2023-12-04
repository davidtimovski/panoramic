using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Models;

namespace Panoramic.Services.Storage.Models;

public class RecentLinksWidgetData : WidgetData
{
    public RecentLinksWidgetData()
        : base(WidgetType.RecentLinks)
    {
    }

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("resetEveryDay")]
    public bool ResetEveryDay { get; set; }

    [JsonPropertyName("links")]
    public List<RecentLink> Links { get; set; } = new();
}

public class RecentLink
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("clicked")]
    public DateTime Clicked { get; set; }
}
