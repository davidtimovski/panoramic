using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionData data)
    : ObservableObject, ISettingsViewModel
{
    private readonly IStorageService _storageService = storageService;
    private event EventHandler<ValidationEventArgs>? Validated;

    public Guid Id { get; } = data.Id;

    [ObservableProperty]
    private Area area = data.Area;

    private string title = data.Title;
    public string Title
    {
        get => title;
        set
        {
            if (SetProperty(ref title, value))
            {
                OnPropertyChanged(nameof(Title));
                Validate();
            }
        }
    }

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validate();
    }

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

    private void Validate() => Validated?.Invoke(this, new ValidationEventArgs(Title.Trim().Length > 0));
}
