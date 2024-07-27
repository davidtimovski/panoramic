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
using Panoramic.Services.Notes;
using Panoramic.Services.Storage;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets;
using Panoramic.ViewModels.Widgets.Checklist;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.Note;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class EditWidgetDialog : Page
{
    private readonly IWidget _widget;
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly AreaPicker _areaPicker;

    private IWidgetForm? widgetForm;
    private Page? settingsContent;
    private ISettingsViewModel? settingsVm;

    public EditWidgetDialog(IWidget widget, IStorageService storageService, INotesOrchestrator notesOrchestrator)
    {
        InitializeComponent();

        _widget = widget;
        _storageService = storageService;
        _notesOrchestrator = notesOrchestrator;

        EditSettingsTitle = "Settings";
        EditAreaTitle = "Area";

        Initialize();

        _areaPicker = new(_storageService, widget.Id);
        _areaPicker.AreaReset += AreaReset;
        _areaPicker.AreaPicked += AreaPicked;
    }

    public string EditSettingsTitle { get; }
    public string EditAreaTitle { get; }

    public event EventHandler<DialogStepChangedEventArgs>? StepChanged;
    public event EventHandler<ValidationEventArgs>? Validated;

    public void AttachSettingsValidationHandler(EventHandler<ValidationEventArgs> handler)
        => settingsVm!.AttachValidationHandler(handler);

    public async Task SubmitAsync()
    {
        if (widgetForm is null)
        {
            return;
        }

        await widgetForm.SubmitAsync();
    }

    private void ShowSettings()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs { DialogTitle = EditSettingsTitle });
        EditSettingsButton.Visibility = Visibility.Collapsed;
        EditAreaButton.Visibility = Visibility.Visible;
        ContentFrame.Content = settingsContent;
    }

    private void ShowAreaPicker()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs { DialogTitle = EditAreaTitle });
        EditAreaButton.Visibility = Visibility.Collapsed;
        EditSettingsButton.Visibility = Visibility.Visible;
        ContentFrame.Content = _areaPicker;
    }

    private void Initialize()
    {
        switch (_widget.Type)
        {
            case WidgetType.Note:
                var noteVm = new NoteSettingsViewModel(_storageService, _notesOrchestrator, ((NoteWidget)_widget).GetData());
                var noteForm = new NoteSettingsForm(noteVm);

                settingsVm = noteVm;
                widgetForm = noteForm;
                settingsContent = noteForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, ((LinkCollectionWidget)_widget).GetData());
                var linkCollectionForm = new LinkCollectionSettingsForm(linkCollectionVm);

                settingsVm = linkCollectionVm;
                widgetForm = linkCollectionForm;
                settingsContent = linkCollectionForm;
                break;
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, ((RecentLinksWidget)_widget).GetData());
                var recentLinksForm = new RecentLinksSettingsForm(recentLinksVm);

                settingsVm = recentLinksVm;
                widgetForm = recentLinksForm;
                settingsContent = recentLinksForm;
                break;
            case WidgetType.Checklist:
                var checklistVm = new ChecklistSettingsViewModel(_storageService, ((ChecklistWidget)_widget).GetData());
                var checklistForm = new ChecklistSettingsForm(checklistVm);

                settingsVm = checklistVm;
                widgetForm = checklistForm;
                settingsContent = checklistForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }

        ContentFrame.Content = settingsContent;
    }

    private void AreaPicked(object? _, AreaPickedEventArgs e)
    {
        settingsVm!.Area = e.Area;
        Validated?.Invoke(this, new ValidationEventArgs { Valid = true });
    }

    private void AreaReset(object? _, EventArgs e) => Validated?.Invoke(this, new ValidationEventArgs { Valid = false });

    private void EditSettingsButton_Click(object _, RoutedEventArgs e) => ShowSettings();

    private void EditAreaButton_Click(object _, RoutedEventArgs e) => ShowAreaPicker();
}
