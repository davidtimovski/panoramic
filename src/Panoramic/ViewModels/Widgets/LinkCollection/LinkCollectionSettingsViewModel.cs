using System;
using System.Threading.Tasks;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class LinkCollectionSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly LinkCollectionWidgetData? _data;

    public LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionWidgetData? data)
        : base("My links", data)
    {
        _storageService = storageService;
        _data = data;
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync(string section)
    {
        if (_data is null)
        {
            var data = new LinkCollectionWidgetData
            {
                Title = Title.Trim()
            };
            await _storageService.AddNewWidgetAsync(section, data);
        }
        else
        {
            _data.Title = Title.Trim();
            await _storageService.SaveWidgetAsync<LinkCollectionWidgetData>(section);
        }
    }
}
