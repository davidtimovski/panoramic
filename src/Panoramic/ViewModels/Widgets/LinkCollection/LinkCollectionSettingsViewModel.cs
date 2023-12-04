using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

// TODO: Validate title non empty,
// find way to fire event to toggle save button
public partial class LinkCollectionSettingsViewModel : ObservableObject
{
    private readonly IStorageService _storageService;

    public LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionWidgetData? data)
    {
        _storageService = storageService;

        if (data is null)
        {
            title = "My links";
        }
        else
        {
            Title = data.Title;
        }
    }

    [ObservableProperty]
    private string title;

    public async Task SubmitAsync(string section)
    {
        await _storageService.AddLinkCollectionWidgetAsync(section, Title).ConfigureAwait(false);
    }
}
