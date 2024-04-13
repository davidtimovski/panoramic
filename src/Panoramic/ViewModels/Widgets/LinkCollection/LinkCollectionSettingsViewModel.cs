using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class LinkCollectionSettingsViewModel(IStorageService storageService, LinkCollectionData data)
    : ObservableObject, ISettingsViewModel
{
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
                OnPropertyChanged();
                Validate();
            }
        }
    }

    [ObservableProperty]
    private string headerHighlight = data.HeaderHighlight.ToString();

    [ObservableProperty]
    private bool searchable = data.Searchable;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validate();
    }

    public async Task SubmitAsync()
    {
        var headerHighlight = Enum.Parse<HeaderHighlight>(HeaderHighlight);

        if (Id == Guid.Empty)
        {
            var widget = new LinkCollectionWidget(storageService, Area, headerHighlight, Title.Trim(), Searchable);
            await storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (LinkCollectionWidget)storageService.Widgets[Id];
            widget.Area = Area;
            widget.HeaderHighlight = headerHighlight;
            widget.Title = Title.Trim();
            widget.Searchable = Searchable;

            await storageService.SaveWidgetAsync(widget);
        }
    }

    private void Validate() => Validated?.Invoke(this, new ValidationEventArgs { Valid = Title.Trim().Length > 0 });
}
