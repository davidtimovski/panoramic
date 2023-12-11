using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Domain;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class EditWidgetDialog : Page
{
    private readonly WidgetData _data;
    private readonly IStorageService _storageService;
    private readonly AreaPicker _areaPicker;

    private IWidgetForm? widgetForm;
    private Page? settingsContent;
    private SettingsViewModel? settingsVm;

    public EditWidgetDialog(WidgetData data, IStorageService storageService)
    {
        InitializeComponent();

        _data = data;
        _storageService = storageService;

        EditSettingsTitle = $"{data.Title}: settings";
        EditAreaTitle = $"{data.Title}: area";

        Initialize();
        _areaPicker = new(_storageService, data.Id);
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
        switch (_data.Type)
        {
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, (RecentLinksWidgetData)_data);
                recentLinksVm.Validated += Validated;

                var recentLinksForm = new RecentLinksSettingsForm(recentLinksVm);

                settingsVm = recentLinksVm;
                widgetForm = recentLinksForm;
                settingsContent = recentLinksForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, (LinkCollectionWidgetData)_data);
                linkCollectionVm.Validated += Validated;

                var linkCollectionForm = new LinkCollectionSettingsForm(linkCollectionVm);

                settingsVm = linkCollectionVm;
                widgetForm = linkCollectionForm;
                settingsContent = linkCollectionForm;
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
