using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using Panoramic.Models;

namespace Panoramic.Utils;

internal static class WidgetUtil
{
    private static readonly FrozenDictionary<string, WidgetType> WidgetTypeMap = new Dictionary<string, WidgetType>
    {
        { "note", WidgetType.Note },
        { "linkcollection", WidgetType.LinkCollection },
        { "recentlinks", WidgetType.RecentLinks },
        { "checklist", WidgetType.Checklist },
    }.ToFrozenDictionary();

    internal static string CreateDataFileName(Guid id, WidgetType type)
    {
        var widgetTypeString = type.ToString().ToLowerInvariant();
        return $"{widgetTypeString}-{id:N}.md";
    }

    internal static WidgetType GetType(string widgetFilePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(widgetFilePath);
        var parts = fileName.Split('-');

        return WidgetTypeMap[parts[0]];
    }
}
