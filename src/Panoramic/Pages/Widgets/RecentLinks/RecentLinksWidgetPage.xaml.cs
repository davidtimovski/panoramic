using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Services;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets.RecentLinks;

public sealed partial class RecentLinksWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public RecentLinksWidgetPage(IServiceProvider serviceProvider, RecentLinksWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _id = widget.Id;

        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        ViewModel = new RecentLinksViewModel(_storageService, eventHub, widget);
    }

    public RecentLinksViewModel ViewModel { get; }

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
