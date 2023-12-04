using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Models.Events;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels.RecentLinks;

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
            case WidgetType.Sample:
                break;
            case WidgetType.RecentLinks:
                var data = (RecentLinksWidgetData)_data;
                var form = new RecentLinksSettingsForm(_section, new RecentLinksSettingsViewModel(_storageService, data));
                widgetForm = form;
                ContentFrame.Content = form;
                break;
            default:
                throw new InvalidOperationException("Unsupported widget type");
        }

        SubmitEnabledChanged?.Invoke(this, new SubmitEnabledChangedEventArgs(true));
    }
}
