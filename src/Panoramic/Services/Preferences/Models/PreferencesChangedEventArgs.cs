using System;
using Microsoft.UI.Xaml;

namespace Panoramic.Services.Preferences.Models;

public sealed class PreferencesChangedEventArgs : EventArgs
{
    public required ApplicationTheme Theme { get; init; }
    public required TimeSpan AutoSaveInterval { get; init; }
    public required TimeSpan AutoSaveMaxDelay { get; init; }
}
