using System.Text.Json.Serialization;

namespace Panoramic.Services.Storage.Models;

public class SampleWidgetData : WidgetData
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
