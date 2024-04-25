using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class ExplorerFolder(string name, FileSystemItemPath path, IReadOnlyList<ExplorerFolder> subfolders, bool isSelected) : ObservableObject
{
    public string Name { get; } = name;
    public FileSystemItemPath Path { get; } = path;
    public IReadOnlyList<ExplorerFolder> Subfolders { get; } = subfolders;

    [ObservableProperty]
    private bool isSelected = isSelected;
}
