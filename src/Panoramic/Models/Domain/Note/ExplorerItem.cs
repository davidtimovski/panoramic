using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.Models.Domain.Note;

public sealed partial class ExplorerItem : ObservableObject
{
    private readonly IStorageService _storageService;

    public ExplorerItem(IStorageService storageService, string name, FileType type, FileSystemItemPath path, IReadOnlyList<ExplorerItem> children)
    {
        _storageService = storageService;

        Name = name;
        Type = type;
        Path = path;
        RenameDeleteVisible = path.Relative == "." ? Visibility.Collapsed : Visibility.Visible;

        foreach (var item in children)
        {
            Children.Add(item);
        }

        IsEnabled = isEnabled;
    }

    public string Name { get; }
    public FileType Type { get; }
    public FileSystemItemPath Path { get; }

    private string? text;
    public string? Text
    {
        get => text;
        set
        {
            if (SetProperty(ref text, value))
            {
                OnPropertyChanged(nameof(Text));
                _storageService.EnqueueNoteWrite(Path.Absolute, value!);
            }
        }
    }

    public ObservableCollection<ExplorerItem> Children = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    private bool isEnabled = true;

    /// <summary>
    /// Disable Rename and Delete context actions if item is root folder.
    /// </summary>
    public Visibility RenameDeleteVisible { get; }

    public double Opacity => IsEnabled ? 1 : 0.5;

    /// <summary>
    /// Used to update the <see cref="Text"/> without raising an event.
    /// </summary>
    public void InitializeContent(string content)
    {
        if (SetProperty(ref text, content))
        {
            OnPropertyChanged(nameof(Text));
        }
    }
}
