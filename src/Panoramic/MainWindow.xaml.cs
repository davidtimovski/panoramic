using System;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.Data.Exceptions;
using Panoramic.Models;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Pages;
using Panoramic.Pages.Widgets;
using Panoramic.Pages.Widgets.Checklist;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.Note;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Preferences;
using Panoramic.Services.Storage;
using Panoramic.ViewModels;

namespace Panoramic;

public sealed partial class MainWindow : Window
{
    private readonly IPreferencesService _preferencesService;
    private readonly IStorageService _storageService;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(
        IPreferencesService preferencesService,
        IStorageService storageService,
        IServiceProvider serviceProvider,
        MainViewModel viewModel)
    {
        InitializeComponent();

        Title = "Panoramic";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        RootElement.Loaded += RootElement_Loaded;
        Closed += WindowClosed;

        _preferencesService = preferencesService;

        _storageService = storageService;
        _storageService.WidgetUpdated += WidgetUpdated;
        _storageService.WidgetDeleted += WidgetDeleted;

        _serviceProvider = serviceProvider;

        ViewModel = viewModel;
    }

    public MainViewModel ViewModel { get; }

    private async void RootElement_Loaded(object _, RoutedEventArgs e)
    {
        try
        {
            await _storageService.ReadAsync();
        }
        catch (MarkdownParsingException ex)
        {
            var content = new MarkdownParsingFailure(ex.FileName!, ex.Lines, ex.PotentialErrorLine);
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Markdown parsing failure",
                Content = content,
                CloseButtonText = "I understand, exit",
                CloseButtonCommand = new RelayCommand(Process.GetCurrentProcess().Kill)
            };
            await dialog.ShowAsync();
            return;
        }

        foreach (var id in _storageService.Widgets.Keys)
        {
            RenderWidget(id);
        }
    }

    private async void WindowClosed(object _, WindowEventArgs args)
    {
        await _storageService.WriteUnsavedChangesAsync();
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

        content.StepChanged += (_, e) => { dialog.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private void WidgetUpdated(object? _, WidgetUpdatedEventArgs e) => RenderWidget(e.Id);

    private void WidgetDeleted(object? _, WidgetDeletedEventArgs e)
    {
        var widget = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == e.Id.ToString("N"));
        Grid.Children.Remove(widget);
    }

    private void RenderWidget(Guid id)
    {
        var widget = _storageService.Widgets[id];
        Page content = widget.Type switch
        {
            WidgetType.Note => new NoteWidgetPage(_serviceProvider, (NoteWidget)widget),
            WidgetType.LinkCollection => new LinkCollectionWidgetPage(_serviceProvider, (LinkCollectionWidget)widget),
            WidgetType.RecentLinks => new RecentLinksWidgetPage(_serviceProvider, (RecentLinksWidget)widget),
            WidgetType.Checklist => new ChecklistWidgetPage(_serviceProvider, (ChecklistWidget)widget),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };

        var name = widget.Id.ToString("N");

        content.SetValue(FrameworkElement.NameProperty, name);
        content.SetValue(Grid.RowProperty, widget.Area.Row);
        content.SetValue(Grid.ColumnProperty, widget.Area.Column);
        content.SetValue(Grid.RowSpanProperty, widget.Area.RowSpan);
        content.SetValue(Grid.ColumnSpanProperty, widget.Area.ColumnSpan);

        var widgetPage = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == name);
        if (widgetPage is not null)
        {
            Grid.Children.Remove(widgetPage);
        }

        Grid.Children.Add(content);
    }

    private async void PreferencesButton_Click(object _, RoutedEventArgs e)
    {
        var content = new PreferencesDialog(_preferencesService, _storageService, this);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Preferences",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(content.ViewModel.Submit)
        };

        content.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private void ControlSHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Text = string.Empty;
        SearchBox.Focus(FocusState.Programmatic);

        args.Handled = true;
    }

    private void EscapeHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (SearchBox.FocusState == FocusState.Unfocused)
        {
            return;
        }

        SearchBox.Text = string.Empty;

        args.Handled = true;
    }

    private void ControlTHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        var checklistWidgets = _storageService.Widgets.Where(x => x.Value.Type == WidgetType.Checklist).ToList();
        if (checklistWidgets.Count != 1)
        {
            return;
        }

        var checklistWidget = checklistWidgets[0].Value;
        var checklistWidgetPage = Grid.Children.OfType<ChecklistWidgetPage>().FirstOrDefault(x => x.Name == checklistWidget.Id.ToString("N"));
        checklistWidgetPage?.OpenNewTaskDialog();

        args.Handled = true;
    }
}
