using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HtmlAgilityPack;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services.Storage;
using Panoramic.Utils;
using Windows.ApplicationModel.DataTransfer;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class EditViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IStorageService _storageService;
    private readonly Guid _id;

    public EditViewModel(
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        IStorageService storageService,
        LinkCollectionData data)
    {
        _dispatcherQueue = dispatcherQueue;
        _httpClient = httpClient;
        _storageService = storageService;
        _id = data.Id;

        foreach (var link in data.Links)
        {
            Links.Add(new EditLinkViewModel(link.Title, link.Uri));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkFormValid))]
    private string newLinkTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkFormValid))]
    private string newLinkUrl = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkTitlePlaceholder))]
    private bool newLinkTitleIsReadOnly;

    [ObservableProperty]
    private string? duplicateLinkTitle;

    public string NewLinkTitlePlaceholder => NewLinkTitleIsReadOnly ? "Loading.." : "Title";

    public bool NewLinkFormValid => NewLinkTitle.Trim().Length > 0 && UriHelper.Create(NewLinkUrl) is not null;

    public ObservableCollection<EditLinkViewModel> Links { get; } = [];

    public async void PasteNewLinkUrl(object _, TextControlPasteEventArgs e)
    {
        if (NewLinkTitle.Trim().Length > 0)
        {
            return;
        }

        NewLinkTitleIsReadOnly = true;

        var package = Clipboard.GetContent();
        if (!package.Contains(StandardDataFormats.Text))
        {
            NewLinkTitleIsReadOnly = false;
            return;
        }

        var text = await package.GetTextAsync();
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//head");
            var node = htmlBody.Element("title");

            _dispatcherQueue.TryEnqueue(() =>
            {
                NewLinkTitle = node.InnerText;
                NewLinkTitleIsReadOnly = false;
            });
        }
        else
        {
            NewLinkTitleIsReadOnly = false;
        }
    }

    public bool UrlExists()
    {
        var existing = Links.FirstOrDefault(x => string.Equals(x.Url, NewLinkUrl, StringComparison.Ordinal));
        if (existing is null)
        {
            DuplicateLinkTitle = null;
            return false;
        }

        DuplicateLinkTitle = $@"""{existing.Title}""";
        return true;
    }

    public void Add()
    {
        Links.Add(new EditLinkViewModel(NewLinkTitle.Trim(), UriHelper.Create(NewLinkUrl.Trim())!));
        NewLinkTitle = string.Empty;
        NewLinkUrl = string.Empty;
        DuplicateLinkTitle = null;
    }

    public void Delete(EditLinkViewModel viewModel)
    {
        Links.Remove(viewModel);
    }

    public async Task SaveAsync()
    {
        var widget = (LinkCollectionWidget)_storageService.Widgets[_id];

        var links = new List<LinkCollectionItem>(Links.Count);
        for (short i = 0; i < Links.Count; i++)
        {
            var link = Links[i];

            links.Add(new LinkCollectionItem
            {
                Title = link.Title.Trim(),
                Uri = link.Uri!,
                Order = (short)(i + 1)
            });
        }

        widget.Links = links;
        await _storageService.SaveWidgetAsync(widget);
    }
}
