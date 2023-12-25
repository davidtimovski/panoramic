using System;

namespace Panoramic.Utils;

internal static class UriHelper
{
    internal static Uri? Create(string url)
    {
        if (url.Trim().Length == 0)
        {
            return null;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        return null;
    }
}
