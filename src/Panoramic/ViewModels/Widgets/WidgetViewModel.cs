using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.ViewModels.Widgets;

public partial class WidgetViewModel : ObservableObject
{
    private readonly SolidColorBrush _backgroundBrush;
    private readonly SolidColorBrush _backgroundHighlightedBrush;

    protected WidgetViewModel()
    {
        var currentTheme = Application.Current.RequestedTheme.ToString();
        var currentThemeDict = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[currentTheme];

        _backgroundBrush = (currentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
        _backgroundHighlightedBrush = (currentThemeDict["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public SolidColorBrush Background => Highlighted ? _backgroundHighlightedBrush : _backgroundBrush;
}
