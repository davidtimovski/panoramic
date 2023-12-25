using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksData data)
        : base(RecentLinksWidget.DefaultTitle, data)
    {
        _storageService = storageService;
        _id = data.Id;

        if (data is null)
        {
            capacity = 15;
            onlyFromToday = true;
        }
        else
        {
            capacity = data.Capacity;
            onlyFromToday = data.OnlyFromToday;
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    [ObservableProperty]
    private int capacity;

    [ObservableProperty]
    private bool onlyFromToday;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync()
    {
        if (_id == Guid.Empty)
        {
            var widget = new RecentLinksWidget(Area, Title.Trim(), Capacity, OnlyFromToday);
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (RecentLinksWidget)_storageService.Widgets[_id];
            widget.Area = Area;
            widget.Title = Title;
            widget.Capacity = Capacity;
            widget.OnlyFromToday = OnlyFromToday;

            await _storageService.SaveWidgetAsync(_id);
        }
    }
}
