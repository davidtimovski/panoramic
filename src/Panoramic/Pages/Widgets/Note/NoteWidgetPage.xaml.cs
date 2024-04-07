using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Note;
using Panoramic.Services.Markdown;
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

        ViewModel = new NoteViewModel(_storageService, widget);

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
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new NewNoteForm(folder.Path.Absolute);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New note",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateNote(_widget.Id, folder.Path.Absolute, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void AddNoteFromContextMenu_Click(object _, RoutedEventArgs e)
    {
        var content = new NewNoteForm(_storageService.StoragePath);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New note",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateNote(_widget.Id, _storageService.StoragePath, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void AddFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new NewFolderForm(folder.Path.Absolute);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateFolder(_widget.Id, folder.Path.Absolute, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void AddFolderFromContextMenu_Click(object _, RoutedEventArgs e)
    {
        var content = new NewFolderForm(_storageService.StoragePath);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.CreateFolder(_widget.Id, _storageService.StoragePath, content.ViewModel.Name)),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void RenameFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new FolderRenameForm(folder.Path.Absolute);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename folder",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.RenameFolder(folder.Path.Absolute, content.ViewModel.Name))
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void DeleteFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete folder",
            Content = $"""Are you sure want to delete the "{folder.Name}" folder and everything in it?""",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.DeleteFolder(folder.Path.Absolute))
        };

        await dialog.ShowAsync();
    }

    private async void RenameNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var note = (ExplorerItem)menuItem.DataContext;

        var content = new NoteRenameForm(note.Path.Absolute);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename note",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.RenameNote(note.Path.Absolute, content.ViewModel.Name))
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void DeleteNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var note = (ExplorerItem)menuItem.DataContext;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete note",
            Content = $@"Are you sure want to delete the ""{note.Name}"" note?",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.DeleteNote(note.Path.Absolute))
        };

        await dialog.ShowAsync();
    }

    private async void DeleteNoteFromContextMenu_Click(object _, RoutedEventArgs e)
    {
        var note = ViewModel.SelectedNote!;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete note",
            Content = $@"Are you sure want to delete the ""{note.Name}"" note?",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => _storageService.DeleteNote(note.Path.Absolute))
        };

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

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
            Content = "Are you sure want to delete this widget?\n\nAny notes that you have will remain on the file system.",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        await dialog.ShowAsync();
    }
}
