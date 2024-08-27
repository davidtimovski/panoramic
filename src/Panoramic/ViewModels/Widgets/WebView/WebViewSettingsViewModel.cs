using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Models.Domain.WebView;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.WebView;

public sealed partial class WebViewSettingsViewModel(IStorageService storageService, WebViewData data)
    : ObservableObject, ISettingsViewModel
{
    private event EventHandler<ValidationEventArgs>? Validated;

    public Guid Id { get; } = data.Id;

    [ObservableProperty]
    private Area area = data.Area;

    [ObservableProperty]
    private string title = data.Title;
    partial void OnTitleChanged(string value) => ValidateAndEmit();

    [ObservableProperty]
    private string headerHighlight = data.HeaderHighlight.ToString();

    [ObservableProperty]
    private string uri = data.Uri.ToString();
    partial void OnUriChanged(string value) => ValidateAndEmit();

    public void AttachValidationHandler(EventHandler<ValidationEventArgs> handler)
    {
        Validated += handler;
        ValidateAndEmit();
    }

    public async Task SubmitAsync()
    {
        var headerHighlight = Enum.Parse<HighlightColor>(HeaderHighlight);

        if (Id == Guid.Empty)
        {
            var widget = new WebViewWidget(storageService, Area, headerHighlight, Title.Trim(), new Uri(Uri));
            await storageService.AddNewWidgetAsync(widget);
        }
        else
        {
            var widget = (WebViewWidget)storageService.Widgets[Id];
            widget.Area = Area;
            widget.HeaderHighlight = headerHighlight;
            widget.Title = Title;
            widget.Uri = new Uri(Uri);

            await storageService.SaveWidgetAsync(widget);
        }
    }

    private void ValidateAndEmit()
    {
        var valid = Title.Trim().Length > 0 && UriHelper.CreateOrDefault(Uri) is not null;
        Validated?.Invoke(this, new ValidationEventArgs { Valid = valid });
    }
}
