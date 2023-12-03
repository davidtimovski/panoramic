using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Panoramic.Services.Storage.Models;

public class RecentLinksWidgetData : WidgetData
{
    [JsonPropertyName("links")]
    public required List<RecentLink> Links { get; set; }
}

public class RecentLink
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
