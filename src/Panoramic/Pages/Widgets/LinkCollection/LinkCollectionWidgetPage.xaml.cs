using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly Guid _id;

    public LinkCollectionWidgetPage(IServiceProvider serviceProvider, LinkCollectionWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _httpClient = serviceProvider.GetRequiredService<HttpClient>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _id = widget.Id;

        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        ViewModel = new LinkCollectionViewModel(eventHub, widget);
    }

    public LinkCollectionViewModel ViewModel { get; }

    private async void EditButton_Click(object _, RoutedEventArgs e)
    {
        var widget = (LinkCollectionWidget)_storageService.Widgets[_id];
        var data = widget.GetData();
        var vm = new EditViewModel(_httpClient, _dispatcherQueue, _storageService, data);

        var content = new EditDialog(vm);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Edit link collection",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(vm.SaveAsync),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        ViewModel.Highlighted = true;

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        var widget = _storageService.Widgets[_id];

        var content = new EditWidgetDialog(widget, _storageService);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = content.EditSettingsTitle,
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        ViewModel.Highlighted = true;

        await dialog.ShowAsync();
    }

    private async void RemoveButton_Click(object _, RoutedEventArgs e)
    {
        var widget = _storageService.Widgets[_id];

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Remove widget",
            Content = $"Are you sure want to remove {widget.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, remove",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_id); })
        };
        await dialog.ShowAsync();
    }
}
