using System;

namespace Panoramic.Models.Domain;

public interface IWidgetData
{
    Guid Id { get; }
    WidgetType Type { get; }
    Area Area { get; }
}
