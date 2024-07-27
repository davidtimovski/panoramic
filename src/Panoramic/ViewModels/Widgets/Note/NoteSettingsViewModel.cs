using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;
using Panoramic.Services.Notes;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class NoteSettingsViewModel : ObservableObject, ISettingsViewModel
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;

    private event EventHandler<ValidationEventArgs>? Validated;

    public NoteSettingsViewModel(IStorageService storageService, INotesOrchestrator notesOrchestrator, NoteData data)
    {
        _storageService = storageService;
        _notesOrchestrator = notesOrchestrator;

        var fontFamilyOptions = FontFamilyHelper.GetAll();
        foreach (var fontFamilyOption in fontFamilyOptions)
        {
            FontFamilyOptions.Add(fontFamilyOption);
        }

        Id = data.Id;
        area = data.Area;
        headerHighlight = data.HeaderHighlight.ToString();
        fontFamily = data.FontFamily;
        fontSize = data.FontSize.ToString(CultureInfo.InvariantCulture);
        recentNotesCapacity = data.RecentNotesCapacity;
    }

    public Guid Id { get; }

    [ObservableProperty]
    private Area area;

    [ObservableProperty]
    private string headerHighlight;

    public ObservableCollection<string> FontFamilyOptions { get; } = [];

    [ObservableProperty]
    private string fontFamily;

    [ObservableProperty]
    private string fontSize;

    [ObservableProperty]
    private int recentNotesCapacity;

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        Validated?.Invoke(this, new ValidationEventArgs { Valid = true });
    }

    public async Task SubmitAsync()
    {
        var size = double.Parse(FontSize);
        var headerHighlight = Enum.Parse<HighlightColor>(HeaderHighlight);

        if (Id == Guid.Empty)
        {
            var widget = new NoteWidget(_storageService, _notesOrchestrator, Area, headerHighlight, FontFamily, size, RecentNotesCapacity);
            await _storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (NoteWidget)_storageService.Widgets[Id];
            widget.Area = Area;
            widget.HeaderHighlight = headerHighlight;
            widget.FontFamily = FontFamily;
            widget.FontSize = size;
            widget.RecentNotesCapacity = RecentNotesCapacity;

            await _notesOrchestrator.SaveNoteWidgetAsync(widget);
        }
    }
}
