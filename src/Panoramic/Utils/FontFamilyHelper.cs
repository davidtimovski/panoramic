using System.Collections.Frozen;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.Utils;

public static class FontFamilyHelper
{
    private static readonly FrozenDictionary<string, FontFamily> FontFamilies = new Dictionary<string, FontFamily>
    {
        { "Default", FontFamily.XamlAutoFontFamily },
        { "Open Sans", new FontFamily("/Assets/Fonts/OpenSans-Regular.ttf#Open Sans") },
        { "Fira Code", new FontFamily("/Assets/Fonts/FiraCode-Regular.ttf#Fira Code") },
    }.ToFrozenDictionary();

    public static FontFamily Get(string fontFamily) => FontFamilies[fontFamily];

    public static IReadOnlyList<string> GetAll() => FontFamilies.Keys;
}
