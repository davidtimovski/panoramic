using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed partial class RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksData data)
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
    public partial int Capacity { get; set; } = data.Capacity;
    [ObservableProperty]
    public partial bool OnlyFromToday { get; set; } = data.OnlyFromToday;

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
            var widget = new RecentLinksWidget(storageService, Area, headerHighlight, Title.Trim(), Capacity, OnlyFromToday, Searchable);
            await storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (RecentLinksWidget)storageService.Widgets[Id];
            widget.Area = Area;
            widget.HeaderHighlight = headerHighlight;
            widget.Title = Title;
            widget.Capacity = Capacity;
            widget.OnlyFromToday = OnlyFromToday;
            widget.Searchable = Searchable;

            await storageService.SaveWidgetAsync(widget);
        }
    }

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = Title.Trim().Length > 0 });
}
