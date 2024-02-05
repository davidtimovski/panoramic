using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Note;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteViewModel : ObservableObject
{
    private readonly IStorageService _storageService;
    private readonly NoteWidget _widget;

    public NoteViewModel(NoteWidget widget, IStorageService storageService)
    {
        _storageService = storageService;
        _widget = widget;

        ReloadFiles();

        if (_widget.FilePath is not null)
        {
            SetSelectedItem(_widget.FilePath);
        }
    }

    public ObservableCollection<ExplorerItem> ExplorerItems { get; } = [];

    [ObservableProperty]
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

    public void SetSelectedItem(string filePath)
    {
        var relativeFilePath = Path.GetRelativePath(_storageService.StoragePath, filePath);
        _widget.SetSelectedNote(relativeFilePath);
        SelectedNote = _widget.SelectedNote;

        ExplorerVisible = false;
    }

    public void ReloadFiles()
    {
        ExplorerItems.Clear();

        foreach (var item in _storageService.ExplorerItems)
        {
            ExplorerItems.Add(item);
        }
    }

    public void AddItem(string name, string directory, FileType type)
    {
        var newItem = new ExplorerItem { Name = name, Path = Path.Combine(directory, name), Type = type };
        AddItem(newItem, directory, ExplorerItems);
    }

    private static bool AddItem(ExplorerItem item, string directory, ObservableCollection<ExplorerItem> collection)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            var parentDirectory = Path.GetDirectoryName(collection[i].Path);
            if (string.Equals(parentDirectory, directory, StringComparison.OrdinalIgnoreCase))
            {
                var updatedCopy = collection.Concat([item]).OrderBy(x => x.Name).ToList();

                collection.Clear();
                foreach (var copyItem in updatedCopy)
                {
                    collection.Add(copyItem);
                }

                return true;
            }

            for (var j = 0; j < collection[i].Children.Count; j++)
            {
                if (AddItem(item, directory, collection[i].Children))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void RemoveItem(string path) => RemoveItem(path, ExplorerItems);

    private static bool RemoveItem(string path, ObservableCollection<ExplorerItem> collection)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            if (string.Equals(collection[i].Path, path, StringComparison.OrdinalIgnoreCase))
            {
                collection.RemoveAt(i);
                return true;
            }

            for (var j = 0; j < collection[i].Children.Count; j++)
            {
                if (RemoveItem(path, collection[i].Children))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
