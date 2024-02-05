using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionData data)
    : SettingsViewModel(data)
{
    private readonly IStorageService _storageService = storageService;
    public Guid Id { get; } = data.Id;

    [ObservableProperty]
    private string title = data.Title;

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(Title.Trim().Length > 0));

    public async Task SubmitAsync()
    {
        if (Id == Guid.Empty)
        {
            var widget = new LinkCollectionWidget(_storageService, Area, Title.Trim());
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (LinkCollectionWidget)_storageService.Widgets[Id];
            widget.Area = Area;
            widget.Title = Title.Trim();

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
