using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksData data)
    : SettingsViewModel(data)
{
    private readonly IStorageService _storageService = storageService;
    public Guid Id { get; } = data.Id;

    public event EventHandler<ValidationEventArgs>? Validated;

    [ObservableProperty]
    private string title = data.Title;

    [ObservableProperty]
    private int capacity = data.Capacity;

    [ObservableProperty]
    private bool onlyFromToday = data.OnlyFromToday;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(Title.Trim().Length > 0));

    public async Task SubmitAsync()
    {
        if (Id == Guid.Empty)
        {
            var widget = new RecentLinksWidget(_storageService, Area, Title.Trim(), Capacity, OnlyFromToday);
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (RecentLinksWidget)_storageService.Widgets[Id];
            widget.Area = Area;
            widget.Title = Title;
            widget.Capacity = Capacity;
            widget.OnlyFromToday = OnlyFromToday;

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
