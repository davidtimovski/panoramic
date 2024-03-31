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

        selectedTheme = _preferencesService.Theme.ToString();
        originalTheme = SelectedTheme;

        _storageService = storageService;

        storagePath = _storageService.StoragePath;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeInfoLabelForeground))]
    private string selectedTheme;

    public SolidColorBrush ThemeInfoLabelForeground => SelectedTheme != originalTheme
        ? _themeInfoLabelForegroundHighlighted
        : _themeInfoLabelForeground;

    [ObservableProperty]
    private string storagePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InvalidFolder))]
    private bool folderIsValid;

    public bool InvalidFolder => !FolderIsValid;

    public void SetFolder(string path)
    {
        FolderIsValid = DirectoryIsEmpty(path);
        StoragePath = path;
    }

    public void Submit()
    {
        _preferencesService.Theme = Enum.Parse<ApplicationTheme>(SelectedTheme);
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
