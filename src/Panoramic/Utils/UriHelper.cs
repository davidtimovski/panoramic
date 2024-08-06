using System;

namespace Panoramic.Utils;

internal static class UriHelper
{
    /// <summary>
    /// If the URL is not empty creates a <see cref="Uri"/> from it, otherwise returns null.
    /// </summary>
    internal static Uri? CreateOrDefault(string url)
    {
        if (url.Trim().Length == 0)
        {
            return null;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
    }
}
