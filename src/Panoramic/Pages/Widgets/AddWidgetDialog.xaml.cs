using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.Checklist;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.Note;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets.Checklist;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.Note;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class AddWidgetDialog : Page
{
    private const string WidgetPickerTitle = "Add widget - type";
    private const string WidgetSettingsTitle = "Add widget - settings";

    private readonly IStorageService _storageService;
    private readonly AreaPicker _areaPicker;

    private short step;
    private Area? selectedArea;
    private WidgetType? selectedWidgetType;
    private IWidgetForm? widgetForm;

    public AddWidgetDialog(IStorageService storageService)
    {
        InitializeComponent();

        _storageService = storageService;

        _areaPicker = new(_storageService, null);
        _areaPicker.AreaReset += AreaReset;
        _areaPicker.AreaPicked += AreaPicked;

        ShowAreaPicker();
    }

    public const string AreaPickerTitle = "Add widget - choose an area";

    public event EventHandler<DialogStepChangedEventArgs>? StepChanged;
    public event EventHandler<ValidationEventArgs>? Validated;

    public async Task SubmitAsync()
    {
        if (widgetForm is null)
        {
            return;
        }

        await widgetForm.SubmitAsync();
    }

    private void ShowAreaPicker()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs { DialogTitle = AreaPickerTitle });
        ContentFrame.Content = _areaPicker;

        PreviousButton.Visibility = Visibility.Collapsed;
        NextButton.Visibility = Visibility.Visible;
        NextButton.IsEnabled = selectedArea is not null;
    }

    private void ShowWidgetPicker()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs { DialogTitle = WidgetPickerTitle });

        var widgetPicker = new WidgetPicker(selectedWidgetType);
        widgetPicker.WidgetPicked += WidgetPicked;
        widgetPicker.WidgetDeselected += WidgetDeselected;

        ContentFrame.Content = widgetPicker;

        PreviousButton.Visibility = Visibility.Visible;
        NextButton.Visibility = Visibility.Visible;
        NextButton.IsEnabled = selectedWidgetType is not null;
        Validated?.Invoke(this, new ValidationEventArgs { Valid = false });
    }

    private void ShowSettingsForm()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs { DialogTitle = WidgetSettingsTitle });

        switch (selectedWidgetType)
        {
            case WidgetType.Note:
                var noteVm = new NoteSettingsViewModel(_storageService, new NoteData
                {
                    Id = Guid.Empty,
                    Area = selectedArea!
                });
                noteVm.AttachValidationHandler(Validated!);

                var noteForm = new NoteSettingsForm(noteVm);

                widgetForm = noteForm;
                ContentFrame.Content = noteForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, new LinkCollectionData
                {
                    Id = Guid.Empty,
                    Area = selectedArea!,
                    Links = []
                });
                linkCollectionVm.AttachValidationHandler(Validated!);

                var linkCollectionForm = new LinkCollectionSettingsForm(linkCollectionVm);

                widgetForm = linkCollectionForm;
                ContentFrame.Content = linkCollectionForm;
                break;
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, new RecentLinksData
                {
                    Id = Guid.Empty,
                    Area = selectedArea!,
                    Links = []
                });
                recentLinksVm.AttachValidationHandler(Validated!);

                var recentLinksForm = new RecentLinksSettingsForm(recentLinksVm);

                widgetForm = recentLinksForm;
                ContentFrame.Content = recentLinksForm;
                break;
            case WidgetType.Checklist:
                var checklistVm = new ChecklistSettingsViewModel(_storageService, new ChecklistData
                {
                    Id = Guid.Empty,
                    Area = selectedArea!,
                    Tasks = []
                });
                checklistVm.AttachValidationHandler(Validated!);

                var checklistForm = new ChecklistSettingsForm(checklistVm);

                widgetForm = checklistForm;
                ContentFrame.Content = checklistForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }

        NextButton.Visibility = Visibility.Collapsed;
    }

    private void AreaPicked(object? _, AreaPickedEventArgs e)
    {
        selectedArea = e.Area;
        NextButton.IsEnabled = true;
    }

    private void AreaReset(object? _, EventArgs e)
    {
        selectedArea = null;
        NextButton.IsEnabled = false;
    }

    private void WidgetPicked(object? _, WidgetPickedEventArgs e)
    {
        selectedWidgetType = e.Type;
        NextButton.IsEnabled = true;
    }

    private void WidgetDeselected(object? _, EventArgs e)
    {
        selectedWidgetType = null;
        NextButton.IsEnabled = false;
    }

    private void PreviousButton_Click(object _, RoutedEventArgs e)
    {
        switch (step)
        {
            case 1:
                ShowAreaPicker();
                break;
            case 2:
                ShowWidgetPicker();
                break;
        }

        step--;
    }

    private void NextButton_Click(object _, RoutedEventArgs e)
    {
        switch (step)
        {
            case 0:
                ShowWidgetPicker();
                break;
            case 1:
                ShowSettingsForm();
                break;
        }

        step++;
    }
}
