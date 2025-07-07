using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Panoramic.Utils;

internal static class HttpUtil
{
    /// <summary>
    /// Invokes a request to the URI and retrieves the page title element text.
    /// If the response is not successful, returns null.
    /// </summary>
    internal static async Task<string?> TryGetPageTitleAsync(HttpClient httpClient, Uri uri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var htmlHead = htmlDoc.DocumentNode.SelectSingleNode("//head");
        if (htmlHead is null)
        {
            return null;
        }

        var node = htmlHead.Element("title");
        if (node is null)
        {
            return null;
        }

        return node.InnerText;
    }
}
