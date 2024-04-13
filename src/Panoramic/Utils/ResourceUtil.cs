using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Panoramic.Data;

namespace Panoramic.Utils;

internal static class ResourceUtil
{
    private static readonly ApplicationTheme ApplicationTheme;

    static ResourceUtil()
    {
        ApplicationTheme = Application.Current.RequestedTheme;
        var appCurrentThemeDict = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[ApplicationTheme.ToString()];

        WidgetBackground = (appCurrentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
        WidgetHighlightedBackground = (appCurrentThemeDict["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!;
        WidgetForeground = (appCurrentThemeDict["PanoramicWidgetForegroundBrush"] as SolidColorBrush)!;
        HighlightedForeground = (appCurrentThemeDict["PanoramicHighlightedForeground"] as SolidColorBrush)!;
        HighlightedBackground = (appCurrentThemeDict["PanoramicHighlightedBackgroundBrush"] as SolidColorBrush)!;
        PaleTextForeground = (appCurrentThemeDict["PanoramicPaleTextForeground"] as SolidColorBrush)!;

        WidgetHeaderBrushes = new Dictionary<HeaderHighlight, SolidColorBrush>
        {
            { HeaderHighlight.None, (appCurrentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)! },
            { HeaderHighlight.Blue, (appCurrentThemeDict["PanoramicBlueHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Red, (appCurrentThemeDict["PanoramicRedHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Green, (appCurrentThemeDict["PanoramicGreenHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Yellow, (appCurrentThemeDict["PanoramicYellowHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Orange, (appCurrentThemeDict["PanoramicOrangeHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Purple, (appCurrentThemeDict["PanoramicPurpleHeaderHighlight"] as SolidColorBrush)! },
            { HeaderHighlight.Teal, (appCurrentThemeDict["PanoramicTealHeaderHighlight"] as SolidColorBrush)! }
        };
    }

    public static SolidColorBrush WidgetBackground { get; }
    public static SolidColorBrush WidgetHighlightedBackground { get; }
    public static SolidColorBrush WidgetForeground { get; }
    public static SolidColorBrush HighlightedForeground { get; }
    public static SolidColorBrush HighlightedBackground { get; }
    public static SolidColorBrush PaleTextForeground { get; }

    public static readonly IReadOnlyDictionary<HeaderHighlight, SolidColorBrush> WidgetHeaderBrushes;

    public static SolidColorBrush GetBrushFromPage(string resourceName, Page page)
    {
        var pageCurrentThemeDict = (ResourceDictionary)page.Resources.ThemeDictionaries[ApplicationTheme.ToString()];
        return (pageCurrentThemeDict[resourceName] as SolidColorBrush)!;
    }
}
