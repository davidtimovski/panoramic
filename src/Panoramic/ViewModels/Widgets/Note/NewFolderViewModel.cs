using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.Note;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public sealed class NewFolderViewModel : ObservableObject
{
    public NewFolderViewModel(IReadOnlyList<FileSystemItem> fileSystemItems, FileSystemItemPath path, string storagePath)
    {
        var folders = DirectoryToTreeViewNode(fileSystemItems, path, storagePath);
        var rootPath = new FileSystemItemPath(storagePath, storagePath);
        var root = new ExplorerFolder(Path.GetFileName(storagePath), rootPath, folders, isSelected: rootPath.Equals(path));

        ExplorerFolders = [root];
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set
        {
            if (SetProperty(ref name, value))
            {
                OnPropertyChanged();
                Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
            }
        }
    }

    private ExplorerFolder selectedFolder;
    public ExplorerFolder SelectedFolder
    {
        get => selectedFolder;
        set
        {
            if (SetProperty(ref selectedFolder, value))
            {
                OnPropertyChanged();
                Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
            }
        }
    }

    public IReadOnlyList<ExplorerFolder> ExplorerFolders { get; } = [];

    public bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.FolderCanBeCreated(Name.Trim(), SelectedFolder.Path.Absolute);

    private static List<ExplorerFolder> DirectoryToTreeViewNode(IReadOnlyList<FileSystemItem> fileSystemItems, FileSystemItemPath path, string currentPath)
        => fileSystemItems
            .Where(x => x.Type == FileType.Folder && x.Path.Parent.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExplorerFolder(x.Name, x.Path, DirectoryToTreeViewNode(fileSystemItems, path, x.Path.Absolute), isSelected: x.Path.Equals(path)))
            .ToList();
}
