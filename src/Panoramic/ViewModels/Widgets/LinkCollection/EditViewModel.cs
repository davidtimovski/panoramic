using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Models.Events;
using Panoramic.Services.Storage;
using Panoramic.Utils;
using Windows.ApplicationModel.DataTransfer;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class EditViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IStorageService _storageService;
    private readonly LinkCollectionWidget _widget;
    private readonly SolidColorBrush _fieldForegroundBrush;
    private readonly SolidColorBrush _fieldChangedForegroundBrush;

    public EditViewModel(
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        IStorageService storageService,
        LinkCollectionWidget widget,
        Page page)
    {
        _httpClient = httpClient;
        _dispatcherQueue = dispatcherQueue;
        _storageService = storageService;
        _widget = widget;
        _fieldForegroundBrush = ResourceUtil.GetBrushFromPage("FieldForegroundBrush", page);
        _fieldChangedForegroundBrush = ResourceUtil.GetBrushFromPage("FieldChangedForegroundBrush", page);

        foreach (var link in widget.Links)
        {
            var vm = new EditLinkViewModel(link.Title, link.Uri, _fieldForegroundBrush, _fieldChangedForegroundBrush);
            vm.Updated += (object? _, EventArgs e) => { ValidateAndEmit(); };

            Links.Add(vm);
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

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

    public bool NewLinkFormValid => NewLinkTitle.Trim().Length > 0 && UriHelper.CreateOrDefault(NewLinkUrl) is not null;

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
            var pageTitle = await HttpUtil.GetPageTitleAsync(_httpClient, uri).ConfigureAwait(false);

            _dispatcherQueue.TryEnqueue(() =>
            {
                NewLinkTitle = pageTitle;
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
            UriHelper.CreateOrDefault(NewLinkUrl.Trim())!,
            changed: true,
            _fieldForegroundBrush,
            _fieldChangedForegroundBrush);

        Links.Add(newLink);
        NewLinkTitle = string.Empty;
        NewLinkUrl = string.Empty;
        DuplicateLinkTitle = null;
    }

    public void Delete(EditLinkViewModel viewModel)
    {
        Links.Remove(viewModel);
        ValidateAndEmit();
    }

    public async Task SaveAsync()
    {
        short order = 1;
        _widget.Links = Links
            .Select(x => new LinkCollectionItem
            {
                Title = x.Title.Trim(),
                Uri = x.Uri!,
                Order = order++
            }).ToList();

        await _storageService.SaveWidgetAsync(_widget);
    }

    private void ValidateAndEmit()
    {
        var valid = Links.All(x => x.IsValid());
        if (valid)
        {
            foreach (var link in Links)
            {
                var sameTitle = Links.Count(x => string.Equals(x.Title.Trim(), link.Title.Trim(), StringComparison.OrdinalIgnoreCase));
                if (sameTitle > 1)
                {
                    valid = false;
                    break;
                }

                var sameUrl = Links.Count(x => string.Equals(x.Url.Trim(), link.Url.Trim(), StringComparison.Ordinal));
                if (sameUrl > 1)
                {
                    valid = false;
                    break;
                }
            }
        }

        Validated?.Invoke(this, new ValidationEventArgs { Valid = valid });
    }
}
