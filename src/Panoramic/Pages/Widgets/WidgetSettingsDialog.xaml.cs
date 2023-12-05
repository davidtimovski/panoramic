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

        Loaded += PageLoaded;
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public async Task SubmitAsync()
    {
        if (widgetForm is null)
        {
            return;
        }

        await widgetForm.SubmitAsync();
    }

    private void PageLoaded(object _, RoutedEventArgs e)
    {
        switch (_data.Type)
        {
            case WidgetType.RecentLinks:
                var recentLinksVm = new RecentLinksSettingsViewModel(_storageService, (RecentLinksWidgetData)_data);
                recentLinksVm.Validated += Validated;

                var recentLinksForm = new RecentLinksSettingsForm(_section, recentLinksVm);
                widgetForm = recentLinksForm;
                ContentFrame.Content = recentLinksForm;
                break;
            case WidgetType.LinkCollection:
                var linkCollectionVm = new LinkCollectionSettingsViewModel(_storageService, (LinkCollectionWidgetData)_data);
                linkCollectionVm.Validated += Validated;

                var linkCollectionForm = new LinkCollectionSettingsForm(_section, linkCollectionVm);
                widgetForm = linkCollectionForm;
                ContentFrame.Content = linkCollectionForm;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }
    }
}
