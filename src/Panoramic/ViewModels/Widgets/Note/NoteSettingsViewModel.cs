using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteSettingsViewModel : SettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public NoteSettingsViewModel(IStorageService storageService, NoteData data)
        : base(NoteWidget.DefaultTitle, data)
    {
        _storageService = storageService;
        _id = data.Id;
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
            var widget = new NoteWidget(Area, Title.Trim());
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = _storageService.Widgets[_id];
            widget.Area = Area;
            widget.Title = Title;

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
