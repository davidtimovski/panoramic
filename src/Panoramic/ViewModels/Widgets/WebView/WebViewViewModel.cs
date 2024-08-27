using System;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.WebView;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets.WebView;

public sealed partial class WebViewViewModel : WidgetViewModel
{
    public WebViewViewModel(WebViewWidget widget)
    {
        HeaderBackgroundBrush = ResourceUtil.HighlightBrushes[widget.HeaderHighlight];
        Title = widget.Title;
        uri = widget.Uri;
    }

    public SolidColorBrush HeaderBackgroundBrush { get; }

    public string Title { get; }

    private Uri uri;
    public Uri Uri
    {
        get => uri;
        set
        {
            SetProperty(ref uri, value);

            // Force an update of the property in order to refresh the page even when Uri has not changed
            OnPropertyChanged();
        }
    }

    public void Refresh()
    {
        Uri = new Uri(Uri.ToString());
    }
}
