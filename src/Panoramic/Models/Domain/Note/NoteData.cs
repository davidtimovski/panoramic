using Panoramic.Utils.Serialization;
using System.Text.Json.Serialization;
using System;

namespace Panoramic.Models.Domain.Note;

public class NoteData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public required WidgetType Type { get; init; }

    [JsonPropertyName("area")]
    [JsonConverter(typeof(AreaJsonConverter))]
    public required Area Area { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
