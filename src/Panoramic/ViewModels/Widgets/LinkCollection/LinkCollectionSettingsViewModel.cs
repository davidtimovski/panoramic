using System;
using System.Threading.Tasks;
using Panoramic.Models.Domain;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class LinkCollectionSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly LinkCollectionWidgetData _data;

    public LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionWidgetData data)
        : base("My links", data)
    {
        _storageService = storageService;
        _data = data;
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync()
    {
        if (_data.Id == Guid.Empty)
        {
            var data = new LinkCollectionWidgetData
            {
                Id = Guid.NewGuid(),
                Area = Area,
                Title = Title.Trim(),
                Links = new()
            };
            await _storageService.AddNewWidgetAsync(data);
        }
        else
        {
            _data.Area = Area;
            _data.Title = Title.Trim();
            await _storageService.SaveWidgetAsync<LinkCollectionWidgetData>(_data.Id);
        }
    }
}
