using Microsoft.UI.Xaml;

namespace Panoramic.Services.Preferences;

public interface IPreferencesService
{
    ApplicationTheme Theme { get; set; }

    void Save();
}
