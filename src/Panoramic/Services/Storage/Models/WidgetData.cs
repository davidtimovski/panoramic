using System.Text.Json.Serialization;

namespace Panoramic.Services.Storage.Models;

public abstract class WidgetData
{
    [JsonPropertyName("type")]
    public required WidgetType Type { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
