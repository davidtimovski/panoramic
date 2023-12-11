using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Panoramic.Models.Domain;

namespace Panoramic.Utils.Serialization;

public class AreaJsonConverter : JsonConverter<Area>
{
    public override Area Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var areaName = reader.GetString()!;
        return new(areaName);
    }

    public override void Write(Utf8JsonWriter writer, Area area, JsonSerializerOptions options)
        => writer.WriteStringValue(area.Name);
}
