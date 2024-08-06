using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Panoramic.Utils;

internal static class HttpUtil
{
    /// <summary>
    /// Invokes a request to the URI and retrieves the page title element text.
    /// </summary>
    internal static async Task<string> GetPageTitleAsync(HttpClient httpClient, Uri uri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await httpClient.SendAsync(request).ConfigureAwait(false);

        var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//head");
        var node = htmlBody.Element("title");

        return node.InnerText;
    }
}
