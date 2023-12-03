using System;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Panoramic.ViewModels;

namespace Panoramic.Pages;

public sealed partial class AddBookmarkDialog : Page
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;

    public AddBookmarkDialog(HttpClient httpClient, DispatcherQueue dispatcherQueue, AddBookmarkViewModel viewModel)
    {
        InitializeComponent();

        _dispatcherQueue = dispatcherQueue;
        _httpClient = httpClient;

        ViewModel = viewModel;

        Loaded += AddBookmarkDialog_Loaded;
    }

    public AddBookmarkViewModel ViewModel { get; }

    private async void AddBookmarkDialog_Loaded(object sender, RoutedEventArgs e)
    {
        var package = Clipboard.GetContent();
        if (!package.Contains(StandardDataFormats.Text))
        {
            return;
        }

        var text = await package.GetTextAsync();
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.Uri = uri.ToString();
            });

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//head");
            var node = htmlBody.Element("title");

            _dispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.Title.Length > 0)
                {
                    return;
                }

                ViewModel.Title = node.InnerText;
            });
        }
    }
}
