using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Services.Preferences;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels;

public sealed partial class PreferencesViewModel : ObservableObject
{
    private readonly SolidColorBrush _themeInfoLabelForeground;
    private readonly SolidColorBrush _themeInfoLabelForegroundHighlighted;

    private readonly IPreferencesService _preferencesService;
    private readonly IStorageService _storageService;
    private readonly string originalTheme;

    public PreferencesViewModel(IPreferencesService preferencesService, IStorageService storageService)
    {
        _themeInfoLabelForeground = ResourceUtil.PaleTextForeground;
        _themeInfoLabelForegroundHighlighted = ResourceUtil.HighlightedForeground;

        _preferencesService = preferencesService;
        _storageService = storageService;
        SelectedTheme = _preferencesService.Theme.ToString();
        originalTheme = SelectedTheme;
        StoragePath = _storageService.StoragePath;
        SelectedAutoSaveInterval = $"{_preferencesService.AutoSaveInterval.TotalSeconds} seconds";
        SelectedAutoSaveMaxDelay = _preferencesService.AutoSaveMaxDelay.TotalSeconds switch
        {
            30 or 45 => $"{_preferencesService.AutoSaveMaxDelay.TotalSeconds} seconds",
            60 => "1 minute",
            120 => "2 minutes",
            180 => "3 minutes",
            300 => "5 minutes",
            _ => throw new InvalidOperationException($"Invalid {nameof(_preferencesService.AutoSaveMaxDelay)} value")
        };
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeInfoLabelForeground))]
    public partial string SelectedTheme { get; set; }

    public SolidColorBrush ThemeInfoLabelForeground => SelectedTheme != originalTheme
        ? _themeInfoLabelForegroundHighlighted
        : _themeInfoLabelForeground;

    [ObservableProperty]
    public partial string StoragePath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InvalidFolder))]
    public partial bool FolderIsValid { get; set; }

    public bool InvalidFolder => !FolderIsValid;

    [ObservableProperty]
    public partial string SelectedAutoSaveInterval { get; set; }

    [ObservableProperty]
    public partial string SelectedAutoSaveMaxDelay { get; set; }

    public void SetFolder(string path)
    {
        FolderIsValid = DirectoryIsEmpty(path);
        StoragePath = path;
    }

    public void Submit()
    {
        _preferencesService.Theme = Enum.Parse<ApplicationTheme>(SelectedTheme);

        _preferencesService.AutoSaveInterval = SelectedAutoSaveInterval switch
        {
            "5 seconds" => TimeSpan.FromSeconds(5),
            "10 seconds" => TimeSpan.FromSeconds(10),
            "15 seconds" => TimeSpan.FromSeconds(15),
            "20 seconds" => TimeSpan.FromSeconds(20),
            "25 seconds" => TimeSpan.FromSeconds(25),
            _ => throw new InvalidOperationException($"Invalid {nameof(_preferencesService.AutoSaveInterval)} value")
        };

        _preferencesService.AutoSaveMaxDelay = SelectedAutoSaveMaxDelay switch
        {
            "30 seconds" => TimeSpan.FromSeconds(30),
            "45 seconds" => TimeSpan.FromSeconds(45),
            "1 minute" => TimeSpan.FromMinutes(1),
            "2 minutes" => TimeSpan.FromMinutes(2),
            "3 minutes" => TimeSpan.FromMinutes(3),
            "5 minutes" => TimeSpan.FromMinutes(5),
            _ => throw new InvalidOperationException($"Invalid {nameof(_preferencesService.AutoSaveMaxDelay)} value")
        };

        _preferencesService.Save();

        _storageService.ChangeStoragePath(StoragePath);
    }

    private static bool DirectoryIsEmpty(string path)
    {
        IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
        using IEnumerator<string> en = items.GetEnumerator();
        return !en.MoveNext();
    }
}
