using System.Threading.Tasks;
using Panoramic.Data;

namespace Panoramic.Models.Domain;

public interface IWidget : IWidgetData
{
    WidgetType Type { get; }

    Task WriteAsync();
    void Delete();
}
