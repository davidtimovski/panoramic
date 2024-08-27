using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.WebView;
using Panoramic.Services.Notes;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.WebView;

namespace Panoramic.Pages.Widgets.WebView;

public sealed partial class WebViewWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly WebViewWidget _widget;

    public WebViewWidgetPage(IServiceProvider serviceProvider, WebViewWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _notesOrchestrator = serviceProvider.GetRequiredService<INotesOrchestrator>();
        _widget = widget;

        ViewModel = new WebViewViewModel(widget);
    }

    public WebViewViewModel ViewModel { get; }

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
            Content = $"Are you sure want to delete {_widget.Title}?",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        await dialog.ShowAsync();
    }
}
