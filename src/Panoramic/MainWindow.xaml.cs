using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.Data;
using Panoramic.Data.Exceptions;
using Panoramic.Models;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Pages;
using Panoramic.Pages.LinkDrawers;
using Panoramic.Pages.Widgets;
using Panoramic.Pages.Widgets.Checklist;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.Note;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services;
using Panoramic.Services.Drawers;
using Panoramic.Services.Drawers.Models;
using Panoramic.Services.Notes;
using Panoramic.Services.Preferences;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels;

namespace Panoramic;

public sealed partial class MainWindow : Window
{
    private readonly IPreferencesService _preferencesService;
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly IDrawerService _drawerService;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;

    public MainWindow(
        IPreferencesService preferencesService,
        IStorageService storageService,
        IEventHub eventHub,
        INotesOrchestrator notesOrchestrator,
        IDrawerService drawerService,
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
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

        _eventHub = eventHub;
        _notesOrchestrator = notesOrchestrator;

        _drawerService = drawerService;
        _drawerService.LinkDrawersLoaded += LinkDrawersLoaded;

        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _dispatcherQueue = dispatcherQueue;

        ViewModel = viewModel;
    }

    public MainViewModel ViewModel { get; }

    private async void RootElement_Loaded(object _, RoutedEventArgs e)
    {
        try
        {
            var noteWidgets = await _notesOrchestrator.ReadWidgetsAsync();
            await _storageService.ReadWidgetsAsync(noteWidgets);
            await _drawerService.ReadLinkDrawersAsync();
        }
        catch (MarkdownParsingException ex)
        {
            var content = new MarkdownParsingFailure(ex.RelativeFilePath, ex.Lines, ex.PotentialErrorLine);
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
        await _notesOrchestrator.WriteUnsavedChangesAsync();
        await _storageService.WriteUnsavedChangesAsync();
    }

    private void WidgetUpdated(object? _, WidgetUpdatedEventArgs e) => RenderWidget(e.Id);

    private void WidgetDeleted(object? _, WidgetDeletedEventArgs e)
    {
        var widget = Grid.Children.OfType<Page>().FirstOrDefault(x => x.Name == e.Id.ToString("N"));
        Grid.Children.Remove(widget);
    }

    private void LinkDrawersLoaded(object? _, LinkDrawersLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            LinkDrawersMenuFlyout.Items.Clear();

            var newMenuItem = new MenuFlyoutItem
            {
                Text = "New",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            newMenuItem.Click += AddLinkDrawer_Click;

            LinkDrawersMenuFlyout.Items.Add(newMenuItem);

            if (e.Drawers.Count == 0)
            {
                return;
            }

            LinkDrawersMenuFlyout.Items.Add(new MenuFlyoutSeparator());

            var orderedDrawers = e.Drawers.OrderBy(x => x.Name).ToList();
            foreach (var drawer in orderedDrawers)
            {
                var drawerMenuItem = new MenuFlyoutItem
                {
                    Text = drawer.Name,
                    Icon = new FontIcon { Glyph = "\uEC59" },
                    DataContext = drawer
                };

                var contextFlyout = new MenuBarItemFlyout();

                var editButton = new MenuFlyoutItem
                {
                    Text = "Edit",
                    DataContext = drawer
                };
                editButton.Click += EditDrawer_Click;
                contextFlyout.Items.Add(editButton);

                var deleteButton = new MenuFlyoutItem
                {
                    Text = "Delete",
                    DataContext = drawer
                };
                deleteButton.Click += DeleteDrawer_Click;
                contextFlyout.Items.Add(deleteButton);

                drawerMenuItem.ContextFlyout = contextFlyout;
                drawerMenuItem.Click += OpenDrawer_Click;

                LinkDrawersMenuFlyout.Items.Add(drawerMenuItem);
            }
        });
    }

    private async void AddLinkDrawer_Click(object _, RoutedEventArgs e)
    {
        var content = new EditDrawerDialog(_httpClient, _dispatcherQueue, _drawerService, data: null);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New link drawer",
            Content = content,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.ViewModel.SaveAsync),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void OpenDrawer_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var drawer = (LinkDrawerData)menuItem.DataContext;

        var content = new ViewDrawerDialog(_eventHub, data: drawer);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = drawer.Name,
            Content = content,
            CloseButtonText = "Close"
        };

        content.ViewModel.LinkClicked += (_, e) => { dialog.Hide(); };

        await dialog.ShowAsync();
    }

    private async void EditDrawer_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var drawer = (LinkDrawerData)menuItem.DataContext;

        var content = new EditDrawerDialog(_httpClient, _dispatcherQueue, _drawerService, data: drawer);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = $"Edit {drawer.Name} drawer",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Close",
            PrimaryButtonCommand = new AsyncRelayCommand(content.ViewModel.SaveAsync),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void DeleteDrawer_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var drawer = (LinkDrawerData)menuItem.DataContext;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = $"Delete {drawer.Name} drawer",
            Content = $"""Are you sure want to delete the "{drawer.Name}" link drawer and everything in it?""",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _drawerService.DeleteLinkDrawer(drawer.Name))
        };

        await dialog.ShowAsync();
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

    private async void AddWidgetButton_Click(object _, RoutedEventArgs e)
    {
        var content = new AddWidgetDialog(_storageService, _notesOrchestrator);
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

    private async void ControlNHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        var noteWidgets = _storageService.Widgets.Where(x => x.Value.Type == WidgetType.Note).ToList();
        if (noteWidgets.Count != 1)
        {
            return;
        }

        args.Handled = true;

        var path = new FileSystemItemPath(_storageService.StoragePath, _storageService.StoragePath);
        var content = new NewNoteForm(_notesOrchestrator.FileSystemItems, path, _storageService.StoragePath);

        var firstNoteWidget = noteWidgets[0].Value;
        void createNote() => _notesOrchestrator.CreateNote(firstNoteWidget.Id, content.ViewModel.SelectedFolder!.Path.Absolute, content.ViewModel.Name);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New note",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(createNote),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            createNote();
            dialog.Hide();
        };

        await dialog.ShowAsync();
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
