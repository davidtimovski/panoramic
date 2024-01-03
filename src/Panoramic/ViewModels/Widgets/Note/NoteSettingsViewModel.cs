using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteSettingsViewModel(IStorageService storageService, NoteData data)
    : SettingsViewModel(NoteWidget.DefaultTitle, data)
{
    private readonly IStorageService _storageService = storageService;
    public Guid Id { get; } = data.Id;

    public event EventHandler<ValidationEventArgs>? Validated;

    [ObservableProperty]
    private int capacity;

    [ObservableProperty]
    private bool onlyFromToday;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(TitleIsValid()));

    public async Task SubmitAsync()
    {
        if (Id == Guid.Empty)
        {
            var widget = new NoteWidget(Area, Title.Trim());
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = _storageService.Widgets[Id];
            widget.Area = Area;
            widget.Title = Title;

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
