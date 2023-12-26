using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain;

public interface IWidget : IWidgetData
{
    Task WriteAsync(string storagePath, JsonSerializerOptions options);
    void Delete(string storagePath);
}
