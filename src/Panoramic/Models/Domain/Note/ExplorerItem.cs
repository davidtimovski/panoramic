using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.Models.Domain.Note;

public sealed partial class ExplorerItem : ObservableObject
{
    private static readonly TimeSpan TextChangeEnqueueDebounceInterval = TimeSpan.FromSeconds(3);

    private readonly DispatcherQueueTimer _debounceTimer;
    private readonly IStorageService _storageService;

    private bool initialized;

    public ExplorerItem(IStorageService storageService, string name, FileType type, FileSystemItemPath path, IReadOnlyList<ExplorerItem> children)
    {
        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;
        _debounceTimer = queue.CreateTimer();

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

    [ObservableProperty]
    private string? text;

    partial void OnTextChanged(string? oldValue, string? newValue)
    {
        if (!initialized)
        {
            initialized = true;
            return;
        }

        _debounceTimer.Debounce(() => _storageService.EnqueueNoteWrite(Path.Absolute, newValue!), TextChangeEnqueueDebounceInterval);
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
}
