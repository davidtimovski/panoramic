using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Note;
using Panoramic.Services.Markdown;
using Panoramic.Services.Notes;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly IMarkdownService _markdownService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly NoteWidget _widget;

    public NoteWidgetPage(IServiceProvider serviceProvider, NoteWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _notesOrchestrator = serviceProvider.GetRequiredService<INotesOrchestrator>();
        _markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _widget = widget;

        _notesOrchestrator.NoteSelectionChanged += NoteSelectionChanged;
        _notesOrchestrator.FileCreated += FileCreated;

        ViewModel = new NoteViewModel(_storageService, _notesOrchestrator, widget);

        SetPresenterContent();
        SetRecentNotes();
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

    private void SetRecentNotes()
    {
        RecentNotesMenuFlyout.Items.Clear();

        if (_widget.RecentNotes.Count == 0)
        {
            var emptyMenuItem = new MenuFlyoutItem
            {
                Text = "No recently opened notes",
                IsEnabled = false,
            };
            RecentNotesMenuFlyout.Items.Add(emptyMenuItem);
            return;
        }

        foreach (var notePath in _widget.RecentNotes)
        {
            var note = NoteViewModel.FindNoteRecursively(notePath, ViewModel.ExplorerItems);
            if (note is not null)
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = notePath.Relative
                };

                menuItem.Click += (_, e) =>
                {
                    ViewModel.SelectedNote = note;
                };
                RecentNotesMenuFlyout.Items.Add(menuItem);
            }
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

        _dispatcherQueue.TryEnqueue(SetRecentNotes);
    }

    private void FileCreated(object? _, FileCreatedEventArgs e)
    {
        ViewModel.AddFile(e.WidgetId, e.Name, e.Type, e.Path);

        _dispatcherQueue.TryEnqueue(() =>
        {
            Editor.Focus(FocusState.Programmatic);
        });
    }

    private async void AddNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new NewNoteForm(_notesOrchestrator.FileSystemItems, folder.Path, _storageService.StoragePath);
        void createNote() => _notesOrchestrator.CreateNote(_widget.Id, content.ViewModel.SelectedFolder!.Path.Absolute, content.ViewModel.Name);

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

    private async void AddNoteFromContextMenu_Click(object _, RoutedEventArgs e)
    {
        var path = new FileSystemItemPath(_storageService.StoragePath, _storageService.StoragePath);
        var content = new NewNoteForm(_notesOrchestrator.FileSystemItems, path, _storageService.StoragePath);
        void createNote() => _notesOrchestrator.CreateNote(_widget.Id, content.ViewModel.SelectedFolder!.Path.Absolute, content.ViewModel.Name);

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

    private async void AddFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new NewFolderForm(_notesOrchestrator.FileSystemItems, folder.Path, _storageService.StoragePath);
        void createFolder() => _notesOrchestrator.CreateFolder(_widget.Id, content.ViewModel.SelectedFolder!.Path.Absolute, content.ViewModel.Name);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(createFolder),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            createFolder();
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void AddFolderFromContextMenu_Click(object _, RoutedEventArgs e)
    {
        var path = new FileSystemItemPath(_storageService.StoragePath, _storageService.StoragePath);
        var content = new NewFolderForm(_notesOrchestrator.FileSystemItems, path, _storageService.StoragePath);
        void createFolder() => _notesOrchestrator.CreateFolder(_widget.Id, content.ViewModel.SelectedFolder!.Path.Absolute, content.ViewModel.Name);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(createFolder),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            createFolder();
            dialog.Hide();
        };

        await dialog.ShowAsync();
    }

    private async void RenameFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var folder = (ExplorerItem)menuItem.DataContext;

        var content = new FolderRenameForm(folder.Path.Absolute);
        void renameFolder() => _notesOrchestrator.RenameFolder(folder.Path, content.ViewModel.Name);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename folder",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(renameFolder)
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            renameFolder();
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
            PrimaryButtonCommand = new RelayCommand(() => _notesOrchestrator.DeleteFolder(folder.Path))
        };

        await dialog.ShowAsync();
    }

    private async void RenameNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var note = (ExplorerItem)menuItem.DataContext;

        var content = new NoteRenameForm(note.Path.Absolute);
        void renameNote() => _notesOrchestrator.RenameNote(note.Path, content.ViewModel.Name);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename note",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(renameNote)
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            renameNote();
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
            PrimaryButtonCommand = new RelayCommand(() => _notesOrchestrator.DeleteNote(note.Path))
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
            PrimaryButtonCommand = new RelayCommand(() => _notesOrchestrator.DeleteNote(note.Path))
        };

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
            Content = "Are you sure want to delete this widget?\n\nAny notes that you have will remain on the file system.",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        await dialog.ShowAsync();
    }
}
