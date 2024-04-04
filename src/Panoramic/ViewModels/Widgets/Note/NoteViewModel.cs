using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Note;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class NoteViewModel : WidgetViewModel
{
    private readonly IStorageService _storageService;
    private readonly NoteWidget _widget;
    private readonly bool _initialized;

    public NoteViewModel(IStorageService storageService, NoteWidget widget)
    {
        _storageService = storageService;
        _storageService.FileCreated += FileCreated;
        _storageService.FileDeleted += FileDeleted;
        _storageService.ItemRenamed += ItemRenamed;
        _storageService.NoteSelectionChanged += NoteSelectionChanged;
        _storageService.NoteContentChanged += NoteContentChanged;
        _storageService.StoragePathChanged += StoragePathChanged;

        _widget = widget;

        ReloadFiles();

        fontFamily = FontFamilyHelper.Get(_widget.FontFamily);
        fontSize = _widget.FontSize;
        title = "Notes";

        SelectedNote = GetSelectedNote(ExplorerItems);

        _initialized = true;
    }

    public ObservableCollection<ExplorerItem> ExplorerItems { get; } = [];

    [ObservableProperty]
    private FontFamily fontFamily;

    [ObservableProperty]
    private double fontSize;

    private ExplorerItem? selectedNote;
    public ExplorerItem? SelectedNote
    {
        get => selectedNote;
        set
        {
            if (value is not null && value.Type == FileType.Folder)
            {
                return;
            }

            var previousPath = _widget.NotePath?.Absolute;

            if (value is not null && value.Text is null)
            {
                value.Text = File.ReadAllText(value.Path!.Absolute);
            }

            if (_initialized)
            {
                _widget.NotePath = value?.Path;

                _storageService.ChangeNoteSelection(_widget.Id, previousPath, value?.Path.Absolute);
                _storageService.EnqueueWidgetWrite(_widget.Id);
            }

            SetProperty(ref selectedNote, value);
            OnPropertyChanged();

            Title = value is null ? "Notes" : value.Name;
            ExplorerVisible = value is null;
            NoteVisible = value is not null;
        }
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool explorerVisible;

    [ObservableProperty]
    private bool noteVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditorVisibility))]
    [NotifyPropertyChangedFor(nameof(PresenterVisibility))]
    [NotifyPropertyChangedFor(nameof(EditToggleTooltip))]
    private bool editing;

    [ObservableProperty]
    private Visibility tipVisibility;

    public Visibility EditorVisibility => Editing ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PresenterVisibility => Editing ? Visibility.Collapsed : Visibility.Visible;

    public string EditToggleTooltip => Editing ? "Preview" : "Edit";

    public void DeselectNote()
    {
        _storageService.ChangeNoteContent(_widget.Id, SelectedNote!.Path.Absolute, SelectedNote!.Text!);

        SelectedNote = null;
    }

    private void ReloadFiles()
    {
        ExplorerItems.Clear();

        var items = _widget.GetExplorerItems();
        foreach (var item in items)
        {
            ExplorerItems.Add(item);
        }

        TipVisibility = ExplorerItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private ExplorerItem? GetSelectedNote(ObservableCollection<ExplorerItem> items)
    {
        if (_widget.NotePath is null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item.Type == FileType.Folder)
            {
                var found = GetSelectedNote(item.Children);
                if (found is not null)
                {
                    return found;
                }
            }

            if (item.Path.Equals(_widget.NotePath))
            {
                return item;
            }
        }

        return null;
    }

    private static void UpdateNoteSelection(ObservableCollection<ExplorerItem> items, string? previousFilePath, string? newFilePath)
    {
        foreach (var item in items)
        {
            if (item.Type == FileType.Folder)
            {
                UpdateNoteSelection(item.Children, previousFilePath, newFilePath);
            }
            else
            {
                if (newFilePath is not null && item.Path.Equals(newFilePath))
                {
                    item.IsEnabled = false;
                }
                else if (previousFilePath is not null && item.Path.Equals(previousFilePath))
                {
                    item.IsEnabled = true;
                }
            }
        }
    }

    private static void UpdateNoteContent(ObservableCollection<ExplorerItem> items, string path, string content)
    {
        foreach (var item in items)
        {
            if (item.Type == FileType.Folder)
            {
                UpdateNoteContent(item.Children, path, content);
            }
            else if (item.Path.Equals(path))
            {
                item.Text = content;
                return;
            }
        }
    }

    private static bool AddItem(ObservableCollection<ExplorerItem> items, ExplorerItem item, string directory)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Type == FileType.Note)
            {
                continue;
            }

            if (items[i].Path.Equals(item.Path.Parent))
            {
                var childrenCopy = items[i].Children.ToList();
                childrenCopy.Add(item);
                var ordered = childrenCopy.OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();

                items[i].Children.Clear();
                foreach (var child in ordered)
                {
                    items[i].Children.Add(child);
                }
                return true;
            }

            for (var j = 0; j < items[i].Children.Count; j++)
            {
                if (AddItem(items[i].Children, item, directory))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool RemoveItem(ObservableCollection<ExplorerItem> items, string path)
    {
        if (SelectedNote is not null && SelectedNote.Path.IsSubPathOf(path))
        {
            DeselectNote();
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Path.Equals(path))
            {
                items.RemoveAt(i);
                return true;
            }

            for (var j = 0; j < items[i].Children.Count; j++)
            {
                if (RemoveItem(items[i].Children, path))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void NoteSelectionChanged(object? _, NoteSelectionChangedEventArgs e)
    {
        if (e.WidgetId == _widget.Id)
        {
            return;
        }

        UpdateNoteSelection(ExplorerItems, e.PreviousFilePath, e.NewFilePath);
    }

    private void NoteContentChanged(object? _, NoteContentChangedEventArgs e)
    {
        if (e.WidgetId == _widget.Id)
        {
            return;
        }

        UpdateNoteContent(ExplorerItems, e.Path, e.Content);
    }

    private void FileCreated(object? _, FileCreatedEventArgs e)
    {
        var noteCreatedInThisWidget = e.Type == FileType.Note && e.WidgetId == _widget.Id;

        var explorerItem = new ExplorerItem(_storageService, e.Name, e.Type, e.Path, [])
        {
            IsEnabled = noteCreatedInThisWidget
        };

        // Add to root level
        if (e.Path.Parent.Equals(_storageService.StoragePath))
        {
            var itemsCopy = ExplorerItems.ToList();
            itemsCopy.Add(explorerItem);
            var ordered = itemsCopy.OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();

            ExplorerItems.Clear();
            foreach (var child in ordered)
            {
                ExplorerItems.Add(child);
            }
        }
        else
        {
            AddItem(ExplorerItems, explorerItem, explorerItem.Path.Parent);
        }

        if (noteCreatedInThisWidget)
        {
            _widget.NotePath = e.Path;
            SelectedNote = GetSelectedNote(ExplorerItems);
        }

        TipVisibility = ExplorerItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void FileDeleted(object? _, FileDeletedEventArgs e)
    {
        if (SelectedNote is not null && SelectedNote.Path.Equals(e.Path))
        {
            DeselectNote();
        }

        RemoveItem(ExplorerItems, e.Path);

        TipVisibility = ExplorerItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ItemRenamed(object? _, EventArgs e) => ReloadFiles();

    private void StoragePathChanged(object? _, EventArgs e) => ReloadFiles();
}
