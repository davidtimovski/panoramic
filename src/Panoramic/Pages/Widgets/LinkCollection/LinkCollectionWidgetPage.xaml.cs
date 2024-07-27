using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services;
using Panoramic.Services.Notes;
using Panoramic.Services.Search;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly LinkCollectionWidget _widget;

    public LinkCollectionWidgetPage(IServiceProvider serviceProvider, LinkCollectionWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _notesOrchestrator = serviceProvider.GetRequiredService<INotesOrchestrator>();
        _httpClient = serviceProvider.GetRequiredService<HttpClient>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _widget = widget;

        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        var searchService = serviceProvider.GetRequiredService<ISearchService>();
        ViewModel = new LinkCollectionViewModel(eventHub, searchService, _dispatcherQueue, widget);
    }

    public LinkCollectionViewModel ViewModel { get; }

    private async void EditButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var vm = new EditViewModel(_httpClient, _dispatcherQueue, _storageService, _widget);

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

        vm.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var content = new EditWidgetDialog(_widget, _storageService, _notesOrchestrator);
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

        content.AttachSettingsValidationHandler((_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; });
        content.StepChanged += (_, e) => { dialog.Title = e.DialogTitle; };

        await dialog.ShowAsync();
    }

    private async void DeleteButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete widget",
            Content = $"Are you sure want to delete {_widget.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        await dialog.ShowAsync();
    }
}
