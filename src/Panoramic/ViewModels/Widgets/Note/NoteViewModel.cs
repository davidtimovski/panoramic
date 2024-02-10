using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Note;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteViewModel : ObservableObject
{
    private readonly IStorageService _storageService;
    private readonly NoteWidget _widget;

    public NoteViewModel(NoteWidget widget, IStorageService storageService)
    {
        _storageService = storageService;
        _storageService.FileCreated += FileCreated;
        _storageService.FileRenamed += FileRenamed;
        _storageService.FileDeleted += FileDeleted;
        _storageService.NoteSelected += NoteSelected;

        _widget = widget;

        ReloadFiles();

        SelectedNote = _widget.SelectedNote;
    }

    public ObservableCollection<ExplorerItem> ExplorerItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(ExplorerVisible))]
    [NotifyPropertyChangedFor(nameof(NoteVisible))]
    private ExplorerItem? selectedNote;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public string Title => SelectedNote is null ? "Explorer" : SelectedNote.Name;
    public bool ExplorerVisible => SelectedNote is null;
    public bool NoteVisible => SelectedNote is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditorVisibility))]
    [NotifyPropertyChangedFor(nameof(PresenterVisibility))]
    [NotifyPropertyChangedFor(nameof(EditToggleTooltip))]
    private bool editing;

    public Visibility EditorVisibility => Editing ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PresenterVisibility => Editing ? Visibility.Collapsed : Visibility.Visible;

    public string EditToggleTooltip => Editing ? "Stop editing" : "Edit";

    public SolidColorBrush Background => Highlighted
        ? (Application.Current.Resources["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!
        : (Application.Current.Resources["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;

    public void SelectNote(string notePath)
        => _widget.SetSelectedNote(notePath);

    public void DeselectNote()
        => _widget.SetSelectedNote(null);

    private void ReloadFiles()
    {
        ExplorerItems.Clear();

        var root = _widget.GetExplorerItems()[0];
        ExplorerItems.Add(root);
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

    private void NoteSelected(object? _, NoteSelectedEventArgs e)
    {
        if (e.WidgetId == _widget.Id)
        {
            SelectedNote = _widget.SelectedNote;
            return;
        }

        UpdateNoteSelection(ExplorerItems, e.PreviousFilePath, e.NewFilePath);
    }

    private void FileCreated(object? _, FileCreatedEventArgs e)
    {
        var path = new FileSystemItemPath(e.Path, _storageService.StoragePath);
        var explorerItem = new ExplorerItem(path, [])
        {
            Name = e.Name,
            Type = e.Type
        };

        AddItem(ExplorerItems, explorerItem, explorerItem.Path.Parent);

        if (e.WidgetId == _widget.Id && e.Type == FileType.Note)
        {
            SelectNote(e.Path);
        }
    }

    private void FileRenamed(object? _, EventArgs e) => ReloadFiles();

    private void FileDeleted(object? _, FileDeletedEventArgs e) => RemoveItem(ExplorerItems, e.Path);
}
