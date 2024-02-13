using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels;

public partial class PreferencesViewModel : ObservableObject
{
    private readonly IStorageService _storageService;

    public PreferencesViewModel(IStorageService storageService)
    {
        _storageService = storageService;

        storagePath = _storageService.StoragePath;
    }

    [ObservableProperty]
    private string storagePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InvalidFolder))]
    private bool folderIsValid;

    public bool InvalidFolder => !FolderIsValid;

    public void SetFolder(string path)
    {
        FolderIsValid = DirectoryIsEmpty(path);
        StoragePath = path;
    }

    public void Submit() => _storageService.ChangeStoragePath(StoragePath);

    private static bool DirectoryIsEmpty(string path)
    {
        IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
        using IEnumerator<string> en = items.GetEnumerator();
        return !en.MoveNext();
    }
}
