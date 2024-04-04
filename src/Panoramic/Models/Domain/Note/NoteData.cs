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
    public required Area Area { get; init; }

    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; init; } = "Default";

    [JsonPropertyName("fontSize")]
    public double FontSize { get; init; } = 15;

    [JsonPropertyName("relativeFilePath")]
    public string? RelativeFilePath { get; init; }
}
