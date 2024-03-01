using System;
using System.Text.Json.Serialization;
using Panoramic.Utils.Serialization;

namespace Panoramic.Models.Domain.Note;

public sealed class NoteData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; } = WidgetType.Note;

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; } = "Default";

    [JsonPropertyName("fontSize")]
    public double FontSize { get; set; } = 15;

    [JsonPropertyName("relativeFilePath")]
    public string? RelativeFilePath { get; set; }
}
