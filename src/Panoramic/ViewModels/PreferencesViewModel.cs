using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services;

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

    public void Submit()
    {
        _storageService.ChangeStoragePath(StoragePath);
    }
}
