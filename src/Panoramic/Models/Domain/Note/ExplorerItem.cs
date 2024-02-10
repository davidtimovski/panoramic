using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services;
using Panoramic.Services.Storage;

namespace Panoramic.Models.Domain.Note;

public partial class ExplorerItem : ObservableObject
{
    public ExplorerItem(IReadOnlyList<ExplorerItem> children, bool isEnabled, bool isRoot = false)
    {
        foreach (var item in children)
        {
            Children.Add(item);
        }

        IsEnabled = isEnabled;
        IsRoot = isRoot;
    }

    public ExplorerItem(bool isEnabled)
        : this([], isEnabled)
    {
    }

    public required string Name { get; init; }
    public required FileType Type { get; init; }
    public required FileSystemItemPath Path { get; init; }

    private string? text;
    public string? Text
    {
        get => text;
        set
        {
            text = value;
            LastEdited = DateTime.Now;
        }
    }
    public ObservableCollection<ExplorerItem> Children = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    private bool isEnabled = true;

    public bool IsRoot { get; }

    public double Opacity => IsEnabled ? 1 : 0.5;

    public DateTime? LastEdited { get; private set; }
}
