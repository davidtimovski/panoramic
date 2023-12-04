using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Services.Storage;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionWidget : Page
{
    private readonly string _section;
    private readonly IStorageService _storageService;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;

    public LinkCollectionWidget(
        string section,
        IStorageService storageService,
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        LinkCollectionViewModel viewModel)
    {
        InitializeComponent();

        _section = section;
        _storageService = storageService;
        _httpClient = httpClient;
        _dispatcherQueue = dispatcherQueue;

        ViewModel = viewModel;
    }

    public LinkCollectionViewModel ViewModel { get; }

    private async void AddButton_Click(object _, RoutedEventArgs e)
    {
        var content = new AddLinkDialog(_httpClient, _dispatcherQueue, new AddLinkViewModel());
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Add link",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => ViewModel.AddLink(content.ViewModel.Title, content.ViewModel.Uri))
        };
        await dialog.ShowAsync();
    }

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

        content.SubmitEnabledChanged += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Enabled; };

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
