using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.Models.Domain.Note;

public sealed partial class ExplorerItem : ObservableObject
{
    public ExplorerItem(FileSystemItemPath path, IReadOnlyList<ExplorerItem> children)
    {
        Path = path;
        RenameDeleteVisible = path.Relative == "." ? Visibility.Collapsed : Visibility.Visible;

        foreach (var item in children)
        {
            Children.Add(item);
        }

        IsEnabled = isEnabled;
    }

    public required string Name { get; init; }
    public required FileType Type { get; init; }
    public FileSystemItemPath Path { get; init; }
    public string? Text { get; set; }
    public ObservableCollection<ExplorerItem> Children = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    private bool isEnabled = true;

    /// <summary>
    /// Disable Rename and Delete context actions if item is root folder.
    /// </summary>
    public Visibility RenameDeleteVisible { get; }

    public double Opacity => IsEnabled ? 1 : 0.5;
}
