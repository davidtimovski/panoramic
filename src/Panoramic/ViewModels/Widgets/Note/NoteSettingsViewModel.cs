using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteSettingsViewModel : SettingsViewModel
{
    public NoteSettingsViewModel(IStorageService storageService, NoteData data) 
        : base(data)
    {
        _storageService = storageService;

        var fontFamilyOptions = FontFamilyHelper.GetAll();
        foreach (var fontFamilyOption in  fontFamilyOptions)
        {
            FontFamilyOptions.Add(fontFamilyOption);
        }

        Id = data.Id;
        fontFamily = data.FontFamily;
        fontSize = data.FontSize.ToString();
    }

    private readonly IStorageService _storageService;
    public Guid Id { get; }

    public event EventHandler<ValidationEventArgs>? Validated;

    public ObservableCollection<string> FontFamilyOptions { get; } = new();

    [ObservableProperty]
    private string fontFamily;

    [ObservableProperty]
    private string fontSize;

    public async Task SubmitAsync()
    {
        var fontSize = double.Parse(FontSize);

        if (Id == Guid.Empty)
        {
            var widget = new NoteWidget(_storageService, Area, FontFamily, fontSize);
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (NoteWidget)_storageService.Widgets[Id];
            widget.Area = Area;
            widget.FontFamily = FontFamily;
            widget.FontSize = fontSize;

            await _storageService.SaveWidgetAsync(widget);
        }
    }
}
