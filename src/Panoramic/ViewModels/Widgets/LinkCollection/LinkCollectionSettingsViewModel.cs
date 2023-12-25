using System;
using System.Threading.Tasks;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class LinkCollectionSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionData data)
        : base(LinkCollectionWidget.DefaultTitle, data)
    {
        _storageService = storageService;
        _id = data.Id;
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync()
    {
        if (_id == Guid.Empty)
        {
            var widget = new LinkCollectionWidget(Area, Title.Trim());
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = _storageService.Widgets[_id];
            widget.Area = Area;
            widget.Title = Title.Trim();
            await _storageService.SaveWidgetAsync(_id);
        }
    }
}
