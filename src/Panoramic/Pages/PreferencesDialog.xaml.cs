using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.ViewModels;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Panoramic.Pages;

public sealed partial class PreferencesDialog : Page
{
    private readonly Window _window;

    public PreferencesDialog(IStorageService storageService, Window window)
    {
        InitializeComponent();

        _window = window;

        ViewModel = new PreferencesViewModel(storageService);
    }

    public PreferencesViewModel ViewModel { get; }

    public event EventHandler<ValidationEventArgs>? Validated;

    public void Submit()
    {
        ViewModel.Submit();
    }

    private async void ChangeStoragePathButton_Click(object _, RoutedEventArgs e)
    {
        // Create a folder picker
        var openPicker = new FolderPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WindowNative.GetWindowHandle(_window);

        // Initialize the folder picker with the window handle (HWND).
        InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();
        if (folder is null)
        {
            return;
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PanoramicPickedFolderToken", folder);

        ViewModel.StoragePath = folder.Path;
        Validated?.Invoke(this, new ValidationEventArgs(true));
    }
}
