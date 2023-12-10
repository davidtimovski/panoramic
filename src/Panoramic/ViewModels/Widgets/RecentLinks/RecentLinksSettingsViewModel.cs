using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public partial class RecentLinksSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly RecentLinksWidgetData _data;

    public RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksWidgetData data)
        : base("Recent", data)
    {
        _storageService = storageService;
        _data = data;

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
        if (_data.Id == Guid.Empty)
        {
            var data = new RecentLinksWidgetData
            {
                Id = Guid.NewGuid(),
                Area = Area,
                Title = Title.Trim(),
                Capacity = Capacity,
                OnlyFromToday = OnlyFromToday,
                Links = new()
            };
            await _storageService.AddNewWidgetAsync(data);
        }
        else
        {
            _data.Area = Area;
            _data.Title = Title.Trim();
            _data.Capacity = Capacity;
            _data.OnlyFromToday = OnlyFromToday;
            await _storageService.SaveWidgetAsync<RecentLinksWidgetData>(_data.Id);
        }
    }
}
