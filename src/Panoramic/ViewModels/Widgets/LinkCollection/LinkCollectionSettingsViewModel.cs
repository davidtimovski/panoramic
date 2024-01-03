using System;
using System.Threading.Tasks;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionData data)
    : SettingsViewModel(LinkCollectionWidget.DefaultTitle, data)
{
    private readonly IStorageService _storageService = storageService;
    public Guid Id { get; } = data.Id;

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync()
    {
        if (Id == Guid.Empty)
        {
            var widget = new LinkCollectionWidget(Area, Title.Trim());
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = _storageService.Widgets[Id];
            widget.Area = Area;
            widget.Title = Title.Trim();

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
