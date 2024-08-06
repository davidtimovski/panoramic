using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HtmlAgilityPack;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Panoramic.Data;
using Panoramic.Models.Events;
using Panoramic.Services.Drawers;
using Panoramic.Utils;
using Windows.ApplicationModel.DataTransfer;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class EditDrawerViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IDrawerService _drawerService;
    private readonly LinkDrawerData _data;
    private readonly SolidColorBrush _fieldForegroundBrush;
    private readonly SolidColorBrush _fieldChangedForegroundBrush;
    private readonly bool _isNew;

    public EditDrawerViewModel(
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        IDrawerService drawerService,
        LinkDrawerData? data,
        Page page)
    {
        _httpClient = httpClient;
        _dispatcherQueue = dispatcherQueue;
        _drawerService = drawerService;
        _data = data ?? new LinkDrawerData
        {
            Name = string.Empty,
            Links = []
        };
        _fieldForegroundBrush = ResourceUtil.GetBrushFromPage("FieldForegroundBrush", page);
        _fieldChangedForegroundBrush = ResourceUtil.GetBrushFromPage("FieldChangedForegroundBrush", page);
        _isNew = data is null;

        _originalName = _data.Name;
        Name = _data.Name;
        foreach (var link in _data.Links)
        {
            var searchTermsString = string.Join(", ", link.SearchTerms);
            var vm = new EditLinkViewModel(link.Title, link.Uri, searchTermsString, _fieldForegroundBrush, _fieldChangedForegroundBrush);
            vm.Updated += (object? _, EventArgs e) => { ValidateAndEmit(); };

            Links.Add(vm);
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    private readonly string _originalName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameForegroundBrush))]
    private string name = string.Empty;
    partial void OnNameChanged(string value) => ValidateAndEmit();

    public SolidColorBrush NameForegroundBrush => _isNew || Name.Trim().Equals(_originalName, StringComparison.Ordinal) ? _fieldForegroundBrush : _fieldChangedForegroundBrush;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkFormValid))]
    private string newLinkTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkFormValid))]
    private string newLinkUrl = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewLinkFormValid))]
    private string newLinkSearchTerms = string.Empty;

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
        var existing = Links.FirstOrDefault(x =>
            string.Equals(x.Title.Trim(), NewLinkTitle.Trim(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.Url.Trim(), NewLinkUrl.Trim(), StringComparison.Ordinal));

        if (existing is null)
        {
            DuplicateLinkTitle = null;
            return false;
        }

        DuplicateLinkTitle = $@"""{existing.Title.Trim()}""";
        return true;
    }

    public void Add()
    {
        var newLink = new EditLinkViewModel(
            NewLinkTitle.Trim(),
            UriHelper.Create(NewLinkUrl.Trim())!,
            NewLinkSearchTerms.Trim(),
            changed: true,
            _fieldForegroundBrush,
            _fieldChangedForegroundBrush);

        Links.Add(newLink);
        NewLinkTitle = string.Empty;
        NewLinkUrl = string.Empty;
        NewLinkSearchTerms = string.Empty;
        DuplicateLinkTitle = null;

        ValidateAndEmit();
    }

    public void Delete(EditLinkViewModel viewModel)
    {
        Links.Remove(viewModel);
        ValidateAndEmit();
    }

    public async Task SaveAsync()
    {
        short order = 1;
        var drawer = new LinkDrawerData
        {
            Name = Name.Trim(),
            Links = Links
                .Select(x => new LinkDrawerLinkData
                {
                    Title = x.Title.Trim(),
                    Uri = x.Uri!,
                    Order = order++,
                    SearchTerms = x.GetSearchTerms()
                }).ToList()
        };

        await _drawerService.SaveLinkDrawerAsync(drawer, _originalName);
    }

    private void ValidateAndEmit()
    {
        var trimmedName = Name.Trim();
        var valid = trimmedName.Length > 0 && Links.All(x => x.IsValid()) && _drawerService.LinkDrawerCanBeCreated(trimmedName, _originalName);

        Validated?.Invoke(this, new ValidationEventArgs { Valid = valid });
    }
}
