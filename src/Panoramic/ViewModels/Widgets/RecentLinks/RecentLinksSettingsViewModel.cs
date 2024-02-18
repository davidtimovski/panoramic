using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.RecentLinks;

public sealed partial class RecentLinksSettingsViewModel(IStorageService storageService, RecentLinksData data)
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

    [ObservableProperty]
    private int capacity = data.Capacity;

    [ObservableProperty]
    private bool onlyFromToday = data.OnlyFromToday;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validate();
    }

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

    private void Validate() => Validated?.Invoke(this, new ValidationEventArgs(Title.Trim().Length > 0));
}
