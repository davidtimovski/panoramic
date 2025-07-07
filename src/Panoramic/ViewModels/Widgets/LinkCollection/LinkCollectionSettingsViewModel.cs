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
    public partial Area Area { get; set; } = data.Area;

    [ObservableProperty]
    public partial string Title { get; set; } = data.Title;

    partial void OnTitleChanged(string value) => ValidateAndEmit();

    [ObservableProperty]
    public partial string HeaderHighlight { get; set; } = data.HeaderHighlight.ToString();

    [ObservableProperty]
    public partial bool Searchable { get; set; } = data.Searchable;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        ValidateAndEmit();
    }

    public async Task SubmitAsync()
    {
        var headerHighlight = Enum.Parse<HighlightColor>(HeaderHighlight);

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

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = Title.Trim().Length > 0 });
}
