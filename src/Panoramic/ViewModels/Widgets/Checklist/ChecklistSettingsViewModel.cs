using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class ChecklistSettingsViewModel(IStorageService storageService, ChecklistData data)
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
    private bool searchable = data.Searchable;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validate();
    }

    public async Task SubmitAsync()
    {
        if (Id == Guid.Empty)
        {
            var widget = new ChecklistWidget(storageService, Area, Title.Trim(), Searchable);
            await storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (ChecklistWidget)storageService.Widgets[Id];
            widget.Area = Area;
            widget.Title = Title.Trim();
            widget.Searchable = Searchable;

            await storageService.SaveWidgetAsync(widget);
        }
    }

    private void Validate() => Validated?.Invoke(this, new ValidationEventArgs { Valid = Title.Trim().Length > 0 });
}
