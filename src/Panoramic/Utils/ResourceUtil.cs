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

        HighlightBrushes = new Dictionary<HighlightColor, SolidColorBrush>
        {
            { HighlightColor.None, (appCurrentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)! },
            { HighlightColor.Blue, (appCurrentThemeDict["PanoramicBlueHighlight"] as SolidColorBrush)! },
            { HighlightColor.Red, (appCurrentThemeDict["PanoramicRedHighlight"] as SolidColorBrush)! },
            { HighlightColor.Green, (appCurrentThemeDict["PanoramicGreenHighlight"] as SolidColorBrush)! },
            { HighlightColor.Yellow, (appCurrentThemeDict["PanoramicYellowHighlight"] as SolidColorBrush)! },
            { HighlightColor.Orange, (appCurrentThemeDict["PanoramicOrangeHighlight"] as SolidColorBrush)! },
            { HighlightColor.Purple, (appCurrentThemeDict["PanoramicPurpleHighlight"] as SolidColorBrush)! },
            { HighlightColor.Teal, (appCurrentThemeDict["PanoramicTealHighlight"] as SolidColorBrush)! }
        };
    }

    public static SolidColorBrush WidgetBackground { get; }
    public static SolidColorBrush WidgetHighlightedBackground { get; }
    public static SolidColorBrush WidgetForeground { get; }
    public static SolidColorBrush HighlightedForeground { get; }
    public static SolidColorBrush HighlightedBackground { get; }
    public static SolidColorBrush PaleTextForeground { get; }

    public static readonly IReadOnlyDictionary<HighlightColor, SolidColorBrush> HighlightBrushes;

    public static SolidColorBrush GetBrushFromPage(string resourceName, Page page)
    {
        var pageCurrentThemeDict = (ResourceDictionary)page.Resources.ThemeDictionaries[ApplicationTheme.ToString()];
        return (pageCurrentThemeDict[resourceName] as SolidColorBrush)!;
    }
}
