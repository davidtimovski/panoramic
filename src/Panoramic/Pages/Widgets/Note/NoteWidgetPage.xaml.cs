using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Note;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly IMarkdownService _markdownService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly NoteWidget _widget;

    public NoteWidgetPage(IServiceProvider serviceProvider, NoteWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _storageService.NoteSelectionChanged += NoteSelectionChanged;

        _markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _widget = widget;

        ViewModel = new NoteViewModel(widget, _storageService);

        SetPresenterContent();
    }

    public NoteViewModel ViewModel { get; }

    private void SetPresenterContent()
    {
        if (ViewModel.SelectedNote is null)
        {
            return;
        }

        var paragraphs = _markdownService.TextToMarkdownParagraphs(ViewModel.SelectedNote.Text!, ViewModel.Title, ViewModel.FontSize);
        Presenter.Blocks.Clear();

        foreach (var paragraph in paragraphs)
        {
            Presenter.Blocks.Add(paragraph);
        }
    }

    private void EditButton_Click(object _, RoutedEventArgs e)
    {
        if (ViewModel.Editing)
        {
            Editor.Focus(FocusState.Programmatic);
            Editor.SelectionStart = Editor.Text.Length;
        }
        else
        {
            SetPresenterContent();
        }
    }

    private void NoteSelectionChanged(object? _, NoteSelectionChangedEventArgs e)
    {
        if (e.WidgetId == _widget.Id)
        {
            _dispatcherQueue.TryEnqueue(SetPresenterContent);
        }
    }

    private async void AddNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NewNoteForm(item.Path.Parent);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New note",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateNote(_widget.Id, item.Path.Absolute, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void AddFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NewFolderForm(item.Path.Parent);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateFolder(_widget.Id, item.Path.Absolute, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void RenameFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new FolderRenameForm(item.Path.Absolute);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename folder",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.RenameFolder(item.Path.Absolute, content.ViewModel.Name))
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void DeleteFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete folder",
            Content = $@"Are you sure want to delete the ""{item.Name}"" folder and everything in it?",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.DeleteFolder(item.Path.Absolute))
        };

        await dialog.ShowAsync();
    }

    private async void RenameNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NoteRenameForm(item.Path.Absolute);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename note",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.RenameNote(item.Path.Absolute, content.ViewModel.Name))
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void DeleteNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete note",
            Content = $@"Are you sure want to delete the ""{item.Name}"" note?",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.DeleteNote(item.Path.Absolute))
        };

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        var content = new EditWidgetDialog(_widget, _storageService);
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

        content.AttachSettingsValidationHandler((_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; });
        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };

        ViewModel.Highlighted = true;

        await dialog.ShowAsync();
    }

    private async void RemoveButton_Click(object _, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Remove widget",
            Content = "Are you sure want to remove this widget?\n\nAny notes that you have will remain on the file system.",
            PrimaryButtonText = "Yes, remove",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        ViewModel.Highlighted = true;

        await dialog.ShowAsync();
    }
}
