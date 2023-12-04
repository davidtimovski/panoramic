using System.Text.Json.Serialization;
using Panoramic.Models;

namespace Panoramic.Services.Storage.Models;

public abstract class WidgetData
{
    public WidgetData(WidgetType type)
    {
        Type = type;
    }

    [JsonPropertyName("type")]
    public WidgetType Type { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;
}
