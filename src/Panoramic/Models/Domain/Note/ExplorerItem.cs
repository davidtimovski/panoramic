using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Panoramic.Services.Notes;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Models.Domain.Note;

public sealed partial class ExplorerItem : ObservableObject
{
    private static readonly TimeSpan TextChangeEnqueueDebounceInterval = TimeSpan.FromSeconds(2);

    private readonly DispatcherQueueTimer _debounceTimer;
    private readonly INotesOrchestrator _notesOrchestrator;

    public ExplorerItem(INotesOrchestrator notesOrchestrator, string name, FileType type, FileSystemItemPath path, IReadOnlyList<ExplorerItem> children)
    {
        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;
        _debounceTimer = queue.CreateTimer();

        _notesOrchestrator = notesOrchestrator;

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

    private string text = string.Empty;
    public string Text
    {
        get => text;
        set
        {
            if (!SetProperty(ref text, value))
            {
                return;
            }

            OnPropertyChanged();

            _debounceTimer.Debounce(() => _notesOrchestrator.SetContent(Path, value), TextChangeEnqueueDebounceInterval);
        }
    }

    public readonly ObservableCollection<ExplorerItem> Children = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    private bool isEnabled = true;

    /// <summary>
    /// Disable Rename and Delete context actions if item is root folder.
    /// </summary>
    public Visibility RenameDeleteVisible { get; }

    public double Opacity => IsEnabled ? 1 : 0.5;

    /// <summary>
    /// Set <see cref="Text"/> without enqueuing file save.
    /// </summary>
    public void InitializeText(string text)
    {
        this.text = text;
        OnPropertyChanged();
    }
}
