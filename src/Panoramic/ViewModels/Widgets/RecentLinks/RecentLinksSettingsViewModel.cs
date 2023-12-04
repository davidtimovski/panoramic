using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.RecentLinks;

public partial class RecentLinksSettingsViewModel : ObservableObject
{
    private readonly IStorageService _storageService;

    public RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksWidgetData? data)
    {
        _storageService = storageService;

        if (data is null)
        {
            title = "Recent";
            capacity = 15;
            resetEveryDay = true;
        }
        else
        {
            Title = data.Title;
            capacity = data.Capacity;
            resetEveryDay = data.ResetEveryDay;
        }
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private int capacity;

    [ObservableProperty]
    private bool resetEveryDay;

    public async Task SubmitAsync(string section)
    {
        await _storageService.AddRecentLinksWidgetAsync(section, Title, Capacity, ResetEveryDay).ConfigureAwait(false);
    }
}
