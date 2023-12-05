using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.UserControls;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class AddWidgetDialog : Page
{
    private const string WidgetPickerTitle = "Choose a widget";
    private const string WidgetSettingsTitle = "Widget settings";

    private readonly IStorageService _storageService;
    private readonly SectionPicker _sectionPicker;

    private IWidgetForm? widgetForm;

    public AddWidgetDialog(IStorageService storageService)
    {
        InitializeComponent();

        _storageService = storageService;

        _sectionPicker = new(_storageService);
        _sectionPicker.SectionPicked += SectionPicked;

        ShowSectionPicker();
    }

    public const string SectionPickerTitle = "Select a section";

    public event EventHandler<StepChangedEventArgs>? StepChanged;
    public event EventHandler<ValidationEventArgs>? Validated;

    public async Task SubmitAsync()
    {
        if (widgetForm is null)
        {
            return;
        }

        await widgetForm.SubmitAsync();
    }

    private void ShowSectionPicker()
    {
        StepChanged?.Invoke(this, new StepChangedEventArgs(SectionPickerTitle));
        ContentFrame.Content = _sectionPicker;
    }

    private void SectionPicked(object? _, SectionPickedEventArgs e)
    {
        StepChanged?.Invoke(this, new StepChangedEventArgs(WidgetPickerTitle));
        ShowWidgetPicker(e.Section);
    }

    private void ShowWidgetPicker(string section)
    {
        var widgetPicker = new WidgetPicker(section);
        widgetPicker.WidgetPicked += WidgetPicked;

        ContentFrame.Content = widgetPicker;
    }

    private void WidgetPicked(object? sender, WidgetPickedEventArgs e)
    {
        StepChanged?.Invoke(this, new StepChangedEventArgs(WidgetSettingsTitle));

        switch (e.Type)
        {
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, null);
                recentLinksVm.Validated += Validated;

                var recentLinksForm = new RecentLinksSettingsForm(e.Section, recentLinksVm);
                widgetForm = recentLinksForm;
                ContentFrame.Content = recentLinksForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, null);
                linkCollectionVm.Validated += Validated;

                var linkCollectionForm = new LinkCollectionSettingsForm(e.Section, linkCollectionVm);
                widgetForm = linkCollectionForm;
                ContentFrame.Content = linkCollectionForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }
    }
}

public class StepChangedEventArgs : EventArgs
{
    public StepChangedEventArgs(string dialogTitle)
    {
        DialogTitle = dialogTitle;
    }

    public string DialogTitle { get; }
}
