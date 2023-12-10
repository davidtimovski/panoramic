using System;
using System.Text.Json.Serialization;
using Panoramic.Models;
using Panoramic.Utils.Serialization;

namespace Panoramic.Services.Storage.Models;

public abstract class WidgetData
{
    public WidgetData(WidgetType type)
    {
        Type = type;
    }

    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; }

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
