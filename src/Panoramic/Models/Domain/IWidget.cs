using System.Threading.Tasks;

namespace Panoramic.Models.Domain;

public interface IWidget : IWidgetData
{
    Task WriteAsync();
    void Delete();
}
