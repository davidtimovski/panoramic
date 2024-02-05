using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Note;
using Panoramic.Services;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly IMarkdownService _markdownService;
    private readonly NoteWidget _widget;

    public NoteWidgetPage(IServiceProvider serviceProvider, NoteWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();

        _eventHub = serviceProvider.GetRequiredService<IEventHub>();
        _eventHub.FileCreated += FileCreated;
        _eventHub.FileRenamed += FileRenamed;
        _eventHub.FileDeleted += FileDeleted;

        _markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        _widget = widget;

        ViewModel = new NoteViewModel(widget, _storageService);

        SetPresenterContent();
    }

    public NoteViewModel ViewModel { get; }

    //private void Editor_TextChanged(object _, TextChangedEventArgs e)
    //{
    //    _widget.SelectedNote!.Text = ViewModel.SelectedNote!.Text;
    //}

    private void SetPresenterContent()
    {
        if (ViewModel.SelectedNote?.Text is null)
        {
            return;
        }

        var paragraphs = _markdownService.TextToMarkdownParagraphs(ViewModel.SelectedNote.Text, ViewModel.Title);
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
        }
        else
        {
            SetPresenterContent();
        }
    }

    private void Explorer_SelectionChanged(TreeView _, TreeViewSelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0)
        {
            return;
        }

        var item = args.AddedItems[0] as ExplorerItem;
        if (item!.Type == FileType.File)
        {
            ViewModel.SetSelectedItem(item.Path);
            SetPresenterContent();
            _storageService.EnqueueWidgetWrite(_widget.Id);
        }
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

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

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

    private async void AddNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NewNoteForm(Path.GetDirectoryName(item.Path)!);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New note",
            Content = content,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.CreateNote(item.Path, content.ViewModel.Name);
                _eventHub.RaiseFileCreated(content.ViewModel.Name, item.Path, FileType.File);
            })
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void AddFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NewFolderForm(Path.GetDirectoryName(item.Path)!);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New folder",
            Content = content,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.CreateFolder(item.Path, content.ViewModel.Name);
                _eventHub.RaiseFileCreated(content.ViewModel.Name, item.Path, FileType.Folder);
            })
        };

        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void RenameFolder_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new FolderRenameForm(item.Path);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename folder",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.RenameFolder(item.Path, content.ViewModel.Name);
                _eventHub.RaiseFileRenamed();
            })
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
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.DeleteFolder(item.Path);
                _eventHub.RaiseFileDeleted(item.Path);
            })
        };

        await dialog.ShowAsync();
    }

    private async void RenameNote_Click(object _, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)e.OriginalSource;
        var item = (ExplorerItem)menuItem.DataContext;

        var content = new NoteRenameForm(item.Path);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Rename note",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.RenameNote(item.Path, content.ViewModel.Name);
                _eventHub.RaiseFileRenamed();
            })
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
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                _storageService.DeleteNote(item.Path);
                _eventHub.RaiseFileDeleted(item.Path);
            })
        };

        await dialog.ShowAsync();
    }

    private void FileCreated(object? _, FileCreatedEventArgs e) => ViewModel.AddItem(e.Name, e.Directory, e.Type);

    private void FileRenamed(object? _, EventArgs e) => ViewModel.ReloadFiles();

    private void FileDeleted(object? _, FileDeletedEventArgs e) => ViewModel.RemoveItem(e.Path);
}
