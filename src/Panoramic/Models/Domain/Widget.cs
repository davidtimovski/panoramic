using System;

namespace Panoramic.Models.Domain;

public abstract class Widget
{
    public Widget(Guid id, WidgetType type, Area area, string title)
    {
        Id = id;
        Type = type;
        Area = area;
        Title = title;
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public string Title { get; set; }
}
