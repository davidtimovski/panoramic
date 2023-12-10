using System;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Pages;
using Panoramic.Pages.Widgets;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels;

namespace Panoramic;

public sealed partial class MainWindow : Window
{
    private readonly IStorageService _storageService;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IStorageService storageService, IServiceProvider serviceProvider, MainViewModel viewModel)
    {
        InitializeComponent();

        Title = "Panoramic";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        Closed += WindowClosed;

        _storageService = storageService;
        _storageService.WidgetUpdated += WidgetUpdated;
        _storageService.WidgetRemoved += WidgetRemoved;

        _serviceProvider = serviceProvider;

        ViewModel = viewModel;

        LoadWidgets();
    }

    public MainViewModel ViewModel { get; }

    private async void WindowClosed(object _, WindowEventArgs args)
    {
        await _storageService.WriteAsync();
    }

    private void LoadWidgets()
    {
        foreach (var id in _storageService.Widgets.Keys)
        {
            LoadWidget(id);
        }
    }

    private async void AddWidgetButton_Click(object _, RoutedEventArgs e)
    {
        var content = new AddWidgetDialog(_storageService);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = AddWidgetDialog.AreaPickerTitle,
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync),
            IsPrimaryButtonEnabled = false
        };

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private void WidgetUpdated(object? _, WidgetUpdatedEventArgs e) => LoadWidget(e.Id);

    private void WidgetRemoved(object? _, WidgetRemovedEventArgs e)
    {
        var widget = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == e.Id.ToString("N"));
        Grid.Children.Remove(widget);
    }

    private void LoadWidget(Guid id)
    {
        var data = _storageService.Widgets[id];
        Page content = data.Type switch
        {
            WidgetType.RecentLinks => new RecentLinksWidget(_serviceProvider, (RecentLinksWidgetData)data),
            WidgetType.LinkCollection => new LinkCollectionWidget(_serviceProvider, (LinkCollectionWidgetData)data),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        Area area = (Area)data.Area;

        content.SetValue(Page.NameProperty, data.Id.ToString("N"));
        content.SetValue(Grid.RowProperty, area.Row);
        content.SetValue(Grid.ColumnProperty, area.Column);
        content.SetValue(Grid.RowSpanProperty, area.RowSpan);
        content.SetValue(Grid.ColumnSpanProperty, area.ColumnSpan);

        var widget = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == data.Id.ToString("N"));
        if (widget is not null)
        {
            Grid.Children.Remove(widget);
        }

        Grid.Children.Add(content);
    }

    private async void PreferencesButton_Click(object _, RoutedEventArgs e)
    {
        var content = new PreferencesDialog(_storageService, this);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Preferences",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(content.Submit)
        };

        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }
}