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
    private Area area = data.Area;

    [ObservableProperty]
    private string title = data.Title;
    partial void OnTitleChanged(string value) => ValidateAndEmit();

    [ObservableProperty]
    private string headerHighlight = data.HeaderHighlight.ToString();

    [ObservableProperty]
    private int capacity = data.Capacity;

    [ObservableProperty]
    private bool onlyFromToday = data.OnlyFromToday;

    [ObservableProperty]
    private bool searchable = data.Searchable;

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
