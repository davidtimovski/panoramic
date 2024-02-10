using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Note;
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

        explorerVisible = widget.NotePath is null;

        ReloadFiles();

        SelectedNote = _widget.SelectedNote;
    }

    public ObservableCollection<ExplorerItem> ExplorerItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExplorerNoteToggleIsEnabled))]
    private ExplorerItem? selectedNote;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(ExplorerNoteToggleTooltip))]
    [NotifyPropertyChangedFor(nameof(NoteVisible))]
    private bool explorerVisible;

    public string Title => ExplorerVisible ? "Explorer" : SelectedNote!.Name;
    public string ExplorerNoteToggleTooltip => ExplorerVisible ? "View note" : "View explorer";
    public bool ExplorerNoteToggleIsEnabled => SelectedNote is not null;
    public bool NoteVisible => !ExplorerVisible;

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

    public void SetSelectedNote(string? notePath)
        => _widget.SetSelectedNote(notePath);

    public void ReloadFiles()
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
            if (items[i].Type == FileType.File)
            {
                continue;
            }

            if (items[i].Path.Equals(item.Path.Parent))
            {
                items[i].Children.Add(item);
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
            SetSelectedNote(null);
            ExplorerVisible = true;
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
            return;
        }

        UpdateNoteSelection(ExplorerItems, e.PreviousFilePath, e.NewFilePath);
    }

    private void FileCreated(object? _, FileCreatedEventArgs e)
    {
        var explorerItem = new ExplorerItem(true)
        {
            Name = e.Name,
            Type = e.Type,
            Path = new(e.Path, _storageService.StoragePath)
        };

        AddItem(ExplorerItems, explorerItem, explorerItem.Path.Parent);
    }

    private void FileRenamed(object? _, EventArgs e) => ReloadFiles();

    private void FileDeleted(object? _, FileDeletedEventArgs e) => RemoveItem(ExplorerItems, e.Path);
}
