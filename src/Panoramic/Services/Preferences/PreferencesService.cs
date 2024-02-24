using Microsoft.UI.Xaml;
using Windows.Storage;

namespace Panoramic.Services.Preferences;

public class PreferencesService : IPreferencesService
{
    public PreferencesService()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        object? themeValue = localSettings.Values[nameof(Theme)];

        Theme = themeValue is null ? ApplicationTheme.Dark : (ApplicationTheme)themeValue;
    }

    public ApplicationTheme Theme { get; set; }

    public void Save()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        localSettings.Values[nameof(Theme)] = (int)Theme;
    }
}
