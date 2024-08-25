using System;
using Microsoft.UI.Xaml;
using Panoramic.Services.Preferences.Models;

namespace Panoramic.Services.Preferences;

public interface IPreferencesService
{
    event EventHandler<PreferencesChangedEventArgs>? Changed;

    ApplicationTheme Theme { get; set; }

    /// <summary>
    /// Time between the last change and the scheduled auto-save.
    /// </summary>
    TimeSpan AutoSaveInterval { get; set; }

    /// <summary>
    /// Maximum time between the first enqueued change and the auto-save.
    /// For cases when the save is debounced too many times.
    /// </summary>
    TimeSpan AutoSaveMaxDelay { get; set; }

    void Save();
}
