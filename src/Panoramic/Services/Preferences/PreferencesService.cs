using System;
using Microsoft.UI.Xaml;
using Panoramic.Services.Preferences.Models;
using Windows.Storage;

namespace Panoramic.Services.Preferences;

public class PreferencesService : IPreferencesService
{
    private const double AutoSaveDefaultIntervalSeconds = 15;
    private const double AutoSaveDefaultMaxDelaySeconds = 60;

    public PreferencesService()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        object? themeValue = localSettings.Values[nameof(Theme)];
        Theme = themeValue is null ? ApplicationTheme.Dark : (ApplicationTheme)themeValue;

        object? autoSaveIntervalValue = localSettings.Values[nameof(AutoSaveInterval)];
        AutoSaveInterval = autoSaveIntervalValue is null ? TimeSpan.FromSeconds(AutoSaveDefaultIntervalSeconds) : TimeSpan.FromSeconds((double)autoSaveIntervalValue);

        object? autoSaveMaxDelayValue = localSettings.Values[nameof(AutoSaveMaxDelay)];
        AutoSaveMaxDelay = autoSaveMaxDelayValue is null ? TimeSpan.FromSeconds(AutoSaveDefaultMaxDelaySeconds) : TimeSpan.FromSeconds((double)autoSaveMaxDelayValue);
    }

    public event EventHandler<PreferencesChangedEventArgs>? Changed;

    public ApplicationTheme Theme { get; set; }

    /// <inheritdoc />
    public TimeSpan AutoSaveInterval { get; set; }

    /// <inheritdoc />
    public TimeSpan AutoSaveMaxDelay { get; set; }

    public void Save()
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values[nameof(Theme)] = (int)Theme;
            localSettings.Values[nameof(AutoSaveInterval)] = AutoSaveInterval.TotalSeconds;
            localSettings.Values[nameof(AutoSaveMaxDelay)] = AutoSaveMaxDelay.TotalSeconds;

            Changed?.Invoke(this, new PreferencesChangedEventArgs
            {
                Theme = Theme,
                AutoSaveInterval = AutoSaveInterval,
                AutoSaveMaxDelay = AutoSaveMaxDelay,
            });
        }
        catch (Exception ex)
        {
            throw new PreferencesException(ex);
        }
    }
}
