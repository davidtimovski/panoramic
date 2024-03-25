using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class NoteSettingsViewModel
    : ObservableObject, ISettingsViewModel
{
    public NoteSettingsViewModel(IStorageService storageService, NoteData data)
    {
        _storageService = storageService;

        var fontFamilyOptions = FontFamilyHelper.GetAll();
        foreach (var fontFamilyOption in fontFamilyOptions)
        {
            FontFamilyOptions.Add(fontFamilyOption);
        }

        Id = data.Id;
        area = data.Area;
        fontFamily = data.FontFamily;
        fontSize = data.FontSize.ToString(CultureInfo.InvariantCulture);
    }

    private readonly IStorageService _storageService;
    private event EventHandler<ValidationEventArgs>? Validated;

    public Guid Id { get; }

    [ObservableProperty]
    private Area area;

    public ObservableCollection<string> FontFamilyOptions { get; } = [];

    [ObservableProperty]
    private string fontFamily;

    [ObservableProperty]
    private string fontSize;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validate();
    }

    public async Task SubmitAsync()
    {
        var size = double.Parse(FontSize);

        if (Id == Guid.Empty)
        {
            var widget = new NoteWidget(_storageService, Area, FontFamily, size);
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (NoteWidget)_storageService.Widgets[Id];
            widget.Area = Area;
            widget.FontFamily = FontFamily;
            widget.FontSize = size;

            await _storageService.SaveWidgetAsync(widget);
        }
    }

    private void Validate() => Validated?.Invoke(this, new ValidationEventArgs { Valid = true });
}
