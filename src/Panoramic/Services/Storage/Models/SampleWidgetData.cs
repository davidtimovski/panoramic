using System.Text.Json.Serialization;
using Panoramic.Models;

namespace Panoramic.Services.Storage.Models;

public class SampleWidgetData : WidgetData
{
    public SampleWidgetData()
        : base(WidgetType.Sample)
    {
    }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
