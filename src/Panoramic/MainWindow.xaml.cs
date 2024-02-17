using System;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Pages;
using Panoramic.Pages.Widgets;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.Note;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
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
        var widget = _storageService.Widgets[id];
        Page content = widget.Type switch
        {
            WidgetType.Note => new NoteWidgetPage(_serviceProvider, (NoteWidget)widget),
            WidgetType.LinkCollection => new LinkCollectionWidgetPage(_serviceProvider, (LinkCollectionWidget)widget),
            WidgetType.RecentLinks => new RecentLinksWidgetPage(_serviceProvider, (RecentLinksWidget)widget),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        var name = widget.Id.ToString("N");

        content.SetValue(Page.NameProperty, name);
        content.SetValue(Grid.RowProperty, widget.Area.Row);
        content.SetValue(Grid.ColumnProperty, widget.Area.Column);
        content.SetValue(Grid.RowSpanProperty, widget.Area.RowSpan);
        content.SetValue(Grid.ColumnSpanProperty, widget.Area.ColumnSpan);

        var widgetPage = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == name);
        if (widget is not null)
        {
            Grid.Children.Remove(widgetPage);
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
            PrimaryButtonCommand = new RelayCommand(content.ViewModel.Submit)
        };

        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }
}
