using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class AddWidgetDialog : Page
{
    private const string WidgetPickerTitle = "Add widget: widget type";
    private const string WidgetSettingsTitle = "Add widget: settings";

    private readonly IStorageService _storageService;
    private readonly AreaPicker _areaPicker;

    private short step = 0;
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

    public const string AreaPickerTitle = "Add widget: choose an area";

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
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs(AreaPickerTitle));
        ContentFrame.Content = _areaPicker;

        PreviousButton.Visibility = Visibility.Collapsed;
        NextButton.Visibility = Visibility.Visible;
        NextButton.IsEnabled = selectedArea is not null;
    }

    private void ShowWidgetPicker()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs(WidgetPickerTitle));

        var widgetPicker = new WidgetPicker(selectedArea!, selectedWidgetType);
        widgetPicker.WidgetPicked += WidgetPicked;
        widgetPicker.WidgetDeselected += WidgetDeselected;

        ContentFrame.Content = widgetPicker;

        PreviousButton.Visibility = Visibility.Visible;
        NextButton.Visibility = Visibility.Visible;
        NextButton.IsEnabled = selectedWidgetType is not null;
        Validated?.Invoke(this, new ValidationEventArgs(false));
    }

    private void ShowSettingsForm()
    {
        StepChanged?.Invoke(this, new DialogStepChangedEventArgs(WidgetSettingsTitle));

        switch (selectedWidgetType)
        {
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, RecentLinksWidgetData.New(selectedArea!));
                recentLinksVm.Validated += Validated;

                var recentLinksForm = new RecentLinksSettingsForm(recentLinksVm);
                widgetForm = recentLinksForm;
                ContentFrame.Content = recentLinksForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, LinkCollectionWidgetData.New(selectedArea!));
                linkCollectionVm.Validated += Validated;

                var linkCollectionForm = new LinkCollectionSettingsForm(linkCollectionVm);
                widgetForm = linkCollectionForm;
                ContentFrame.Content = linkCollectionForm;
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
