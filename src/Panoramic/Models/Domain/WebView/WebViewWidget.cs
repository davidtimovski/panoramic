using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.WebView;

public sealed class WebViewWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new web view widget.
    /// </summary>
    public WebViewWidget(IStorageService storageService, Area area, HighlightColor headerHighlight, string title, Uri uri)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.WebView);

        Area = area;
        HeaderHighlight = headerHighlight;
        Title = title;
        Uri = uri;
    }

    /// <summary>
    /// Constructs a web view widget based on existing data.
    /// </summary>
    private WebViewWidget(IStorageService storageService, WebViewData data)
    {
        _storageService = storageService;
        _dataFileName = WidgetUtil.CreateDataFileName(data.Id, WidgetType.WebView);

        Id = data.Id;
        Area = data.Area;
        HeaderHighlight = data.HeaderHighlight;
        Title = data.Title;
        Uri = data.Uri;
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.WebView;
    public Area Area { get; set; }
    public HighlightColor HeaderHighlight { get; set; }
    public string Title { get; set; }
    public Uri Uri { get; set; }

    public WebViewData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            HeaderHighlight = HeaderHighlight,
            Title = Title,
            Uri = Uri
        };

    public static WebViewWidget Load(IStorageService storageService, string relativeFilePath, string markdown)
    {
        var data = WebViewData.FromMarkdown(relativeFilePath, markdown);
        return new(storageService, data);
    }

    public async Task WriteAsync()
    {
        DebugLogger.Log($"Writing out {Type} widget to file system. ID: {Id}.");

        var data = GetData();

        var builder = new StringBuilder();
        data.ToMarkdown(builder);
        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), builder.ToString());
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }
}
