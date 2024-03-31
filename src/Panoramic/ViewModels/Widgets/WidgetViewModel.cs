using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Panoramic.Utils;

namespace Panoramic.ViewModels.Widgets;

public partial class WidgetViewModel : ObservableObject
{
    private readonly SolidColorBrush _backgroundBrush;
    private readonly SolidColorBrush _backgroundHighlightedBrush;

    protected WidgetViewModel()
    {
        _backgroundBrush = ResourceUtil.WidgetBackground;
        _backgroundHighlightedBrush = ResourceUtil.WidgetHighlightedBackground;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public SolidColorBrush Background => Highlighted ? _backgroundHighlightedBrush : _backgroundBrush;
}
