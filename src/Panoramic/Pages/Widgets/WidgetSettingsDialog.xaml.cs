using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.LinkCollection;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels.Widgets.LinkCollection;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class WidgetSettingsDialog : Page
{
    private readonly string _section;
    private readonly WidgetData _data;
    private readonly IStorageService _storageService;

    private IWidgetForm? widgetForm;

    public WidgetSettingsDialog(string section, WidgetData data, IStorageService storageService)
    {
        InitializeComponent();

        _data = data;
        _section = section;
        _storageService = storageService;

        LoadWidgetSettingsForm();
    }

    public event EventHandler<SubmitEnabledChangedEventArgs>? SubmitEnabledChanged;

    public async Task SubmitAsync()
    {
        if (widgetForm is null)
        {
            return;
        }

        await widgetForm.SubmitAsync();
    }

    private void LoadWidgetSettingsForm()
    {
        switch (_data.Type)
        {
            case WidgetType.RecentLinks:
                var recentLinksForm = new RecentLinksSettingsForm(_section, new RecentLinksSettingsViewModel(_storageService, (RecentLinksWidgetData)_data));
                widgetForm = recentLinksForm;
                ContentFrame.Content = recentLinksForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionForm = new LinkCollectionSettingsForm(_section, new LinkCollectionSettingsViewModel(_storageService, (LinkCollectionWidgetData)_data));
                widgetForm = linkCollectionForm;
                ContentFrame.Content = linkCollectionForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }

        // TODO: maybe use to signal validation changes
        //SubmitEnabledChanged?.Invoke(this, new SubmitEnabledChangedEventArgs(true));
    }
}
