using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.Utils;

internal static class ResourceUtil
{
    private static readonly ApplicationTheme ApplicationTheme;

    static ResourceUtil()
    {
        ApplicationTheme = Application.Current.RequestedTheme;
        var appCurrentThemeDict = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[ApplicationTheme.ToString()];

        WidgetBackground = (appCurrentThemeDict["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
        WidgetForeground = (appCurrentThemeDict["PanoramicWidgetForegroundBrush"] as SolidColorBrush)!;
        HighlightedForeground = (appCurrentThemeDict["PanoramicHighlightedForeground"] as SolidColorBrush)!;
        WidgetHighlightedBackground = (appCurrentThemeDict["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!;
        PaleTextForeground = (appCurrentThemeDict["PanoramicPaleTextForeground"] as SolidColorBrush)!;
    }

    public static SolidColorBrush WidgetBackground { get; }
    public static SolidColorBrush WidgetForeground { get; }
    public static SolidColorBrush HighlightedForeground { get; }
    public static SolidColorBrush WidgetHighlightedBackground { get; }
    public static SolidColorBrush PaleTextForeground { get; }

    public static SolidColorBrush GetBrushFromPage(string resourceName, Page page)
    {
        var pageCurrentThemeDict = (ResourceDictionary)page.Resources.ThemeDictionaries[ApplicationTheme.ToString()];
        return (pageCurrentThemeDict[resourceName] as SolidColorBrush)!;
    }
}
