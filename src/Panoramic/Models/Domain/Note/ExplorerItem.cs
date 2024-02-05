using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Panoramic.Models.Domain.Note;

public partial class ExplorerItem : ObservableObject
{
    public required string Name { get; init; }
    public required FileType Type { get; init; }
    public required string Path { get; init; }

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
    private bool isExpanded;

    public DateTime? LastEdited { get; private set; }
}
