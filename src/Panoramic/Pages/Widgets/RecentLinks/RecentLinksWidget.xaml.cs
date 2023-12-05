using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets.RecentLinks;

public sealed partial class RecentLinksWidget : Page
{
    private readonly string _section;
    private readonly IStorageService _storageService;

    public RecentLinksWidget(string section, IStorageService storageService, RecentLinksViewModel viewModel)
    {
        InitializeComponent();

        _section = section;
        _storageService = storageService;

        ViewModel = viewModel;
    }

    public RecentLinksViewModel ViewModel { get; }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        var widgetData = _storageService.Sections[_section];

        var content = new WidgetSettingsDialog(_section, widgetData, _storageService);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"{widgetData.Title} ({_section}) - settings",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync)
        };

        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void RemoveButton_Click(object _, RoutedEventArgs e)
    {
        var widgetData = _storageService.Sections[_section];

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Remove widget",
            Content = $"Are you sure want to remove {widgetData.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, remove",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_section); })
        };
        await dialog.ShowAsync();
    }
}
