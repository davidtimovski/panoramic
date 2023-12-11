using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain;
using Panoramic.Services;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionWidget : Page
{
    private readonly IStorageService _storageService;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly Guid _id;

    public LinkCollectionWidget(IServiceProvider serviceProvider, LinkCollectionWidgetData data)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _httpClient = serviceProvider.GetRequiredService<HttpClient>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _id = data.Id;

        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        ViewModel = new LinkCollectionViewModel(_storageService, eventHub, _dispatcherQueue, data);
    }

    public LinkCollectionViewModel ViewModel { get; }

    private async void AddButton_Click(object _, RoutedEventArgs e)
    {
        var vm = new AddLinkViewModel();
        var content = new AddLinkDialog(_httpClient, _dispatcherQueue, vm);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Add link",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => ViewModel.AddLink(content.ViewModel.Title, content.ViewModel.Uri)),
            IsPrimaryButtonEnabled = false
        };

        vm.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        var widgetData = _storageService.Widgets[_id];

        var content = new EditWidgetDialog(widgetData, _storageService);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = content.EditSettingsTitle,
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync)
        };

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void RemoveButton_Click(object _, RoutedEventArgs e)
    {
        var widgetData = _storageService.Widgets[_id];

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Remove widget",
            Content = $"Are you sure want to remove {widgetData.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, remove",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_id); })
        };
        await dialog.ShowAsync();
    }
}
