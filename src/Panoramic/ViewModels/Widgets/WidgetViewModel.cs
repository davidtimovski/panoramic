using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public partial class WidgetViewModel : ObservableObject
{
    private readonly SolidColorBrush BackgroundBrush;
    private readonly SolidColorBrush BackgroundHighlightedBrush;

    public WidgetViewModel()
    {
        var currentTheme = Application.Current.RequestedTheme.ToString();
        var currentThemeDict = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[currentTheme];

        BackgroundBrush = (currentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
        BackgroundHighlightedBrush = (currentThemeDict["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    public SolidColorBrush Background => Highlighted ? BackgroundHighlightedBrush : BackgroundBrush;
}
