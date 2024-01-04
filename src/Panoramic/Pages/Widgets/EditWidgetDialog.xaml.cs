using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Domain.RecentLinks;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.Note;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.Note;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class EditWidgetDialog : Page
{
    private readonly IWidget _widget;
    private readonly IStorageService _storageService;
    private readonly AreaPicker _areaPicker;

    private IWidgetForm? widgetForm;
    private Page? settingsContent;
    private SettingsViewModel? settingsVm;

    public EditWidgetDialog(IWidget widget, IStorageService storageService)
    {
        InitializeComponent();

        _widget = widget;
        _storageService = storageService;

        EditSettingsTitle = $"{widget.Title} - settings";
        EditAreaTitle = $"{widget.Title} - area";

        Initialize();
        _areaPicker = new(_storageService, widget.Id);
        _areaPicker.AreaReset += AreaReset;
        _areaPicker.AreaPicked += AreaPicked;
    }

    public string EditSettingsTitle { get; }
    public string EditAreaTitle { get; }

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

    private void ShowSettings()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs(EditSettingsTitle));
        EditSettingsButton.Visibility = Visibility.Collapsed;
        EditAreaButton.Visibility = Visibility.Visible;
        ContentFrame.Content = settingsContent;
    }

    private void ShowAreaPicker()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs(EditAreaTitle));
        EditAreaButton.Visibility = Visibility.Collapsed;
        EditSettingsButton.Visibility = Visibility.Visible;
        ContentFrame.Content = _areaPicker;
    }

    private void Initialize()
    {
        switch (_widget.Type)
        {
            case WidgetType.Note:
                var noteVm = new NoteSettingsViewModel(_storageService, ((NoteWidget)_widget).GetData());

                var noteForm = new NoteSettingsForm(noteVm);
                noteForm.ViewModel.Validated += Validated;

                settingsVm = noteVm;
                widgetForm = noteForm;
                settingsContent = noteForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, ((LinkCollectionWidget)_widget).GetData());

                var linkCollectionForm = new LinkCollectionSettingsForm(linkCollectionVm);
                linkCollectionForm.ViewModel.Validated += Validated;

                settingsVm = linkCollectionVm;
                widgetForm = linkCollectionForm;
                settingsContent = linkCollectionForm;
                break;
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, ((RecentLinksWidget)_widget).GetData());

                var recentLinksForm = new RecentLinksSettingsForm(recentLinksVm);
                recentLinksForm.ViewModel.Validated += Validated;

                settingsVm = recentLinksVm;
                widgetForm = recentLinksForm;
                settingsContent = recentLinksForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }

        ContentFrame.Content = settingsContent;
    }

    private void AreaPicked(object? _, AreaPickedEventArgs e)
    {
        settingsVm!.Area = e.Area;
        Validated?.Invoke(this, new ValidationEventArgs(true));
    }

    private void AreaReset(object? _, EventArgs e) => Validated?.Invoke(this, new ValidationEventArgs(false));

    private void EditSettingsButton_Click(object _, RoutedEventArgs e) => ShowSettings();

    private void EditAreaButton_Click(object _, RoutedEventArgs e) => ShowAreaPicker();
}
