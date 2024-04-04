using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Panoramic.Data;

namespace Panoramic.Models.Domain.Checklist;

public sealed class ChecklistData : IWidgetData
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("type")]
    public WidgetType Type { get; } = WidgetType.Checklist;

    [JsonPropertyName("area")]
    public required Area Area { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "To do";

    [JsonRequired]
    [JsonPropertyName("searchable")]
    public bool Searchable { get; init; } = true;

    [JsonRequired]
    [JsonPropertyName("tasks")]
    public required List<ChecklistTaskData> Tasks { get; init; }
}

public sealed class ChecklistTaskData
{
    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonRequired]
    [JsonPropertyName("dueDate")]
    public required DateOnly? DueDate { get; init; }

    [JsonRequired]
    [JsonPropertyName("created")]
    public required DateTime Created { get; init; }
}
