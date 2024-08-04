using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Data.Widgets;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.RecentLinks;

public sealed class RecentLinksWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new recent links widget.
    /// </summary>
    public RecentLinksWidget(IStorageService storageService, Area area, HighlightColor headerHighlight, string title, int capacity, bool onlyFromToday, bool searchable)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.RecentLinks);

        Area = area;
        HeaderHighlight = headerHighlight;
        Title = title;
        Capacity = capacity;
        OnlyFromToday = onlyFromToday;
        Searchable = searchable;
        links = [];
    }

    /// <summary>
    /// Constructs a recent links widget based on existing data.
    /// </summary>
    private RecentLinksWidget(IStorageService storageService, RecentLinksData data)
    {
        _storageService = storageService;
        _dataFileName = WidgetUtil.CreateDataFileName(data.Id, WidgetType.RecentLinks);

        Id = data.Id;
        Area = data.Area;
        HeaderHighlight = data.HeaderHighlight;
        Title = data.Title;
        Capacity = data.Capacity;
        OnlyFromToday = data.OnlyFromToday;
        Searchable = data.Searchable;
        links = data.Links.Select(x => new RecentLink { Title = x.Title, Uri = x.Uri, Context = x.Context, Clicked = x.Clicked }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.RecentLinks;
    public Area Area { get; set; }
    public HighlightColor HeaderHighlight { get; set; }
    public string Title { get; set; }
    public int Capacity { get; set; }
    public bool OnlyFromToday { get; set; }
    public bool Searchable { get; set; }

    private List<RecentLink> links;
    public IReadOnlyList<RecentLink> Links
    {
        get
        {
            var query = OnlyFromToday ? links.Where(x => x.Clicked >= DateTime.Today) : links;
            return query.OrderByDescending(x => x.Clicked).Take(Capacity).ToList();
        }
        private set
        {
            links = [.. value];
        }
    }

    public void HyperlinkClicked(string title, Uri uri, string context, DateTime clicked)
    {
        var clickedLink = new RecentLink { Title = title, Uri = uri, Context = context, Clicked = clicked };

        var link = Links.FirstOrDefault(x => x.Uri.Equals(uri));
        if (link is null)
        {
            links.Add(clickedLink);

            if (Links.Count > Capacity)
            {
                links.RemoveAt(0);
            }
        }
        else
        {
            var index = links.IndexOf(link);
            links[index] = clickedLink;
        }

        var query = Links.AsEnumerable();

        links.Clear();
        links.AddRange(query.OrderByDescending(x => x.Clicked).Take(Capacity));
    }

    public void Clear() => links.Clear();

    public RecentLinksData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            HeaderHighlight = HeaderHighlight,
            Title = Title,
            Capacity = Capacity,
            OnlyFromToday = OnlyFromToday,
            Searchable = Searchable,
            Links = Links.Select(x => new RecentLinkData { Title = x.Title, Uri = x.Uri, Context = x.Context, Clicked = x.Clicked }).ToList()
        };

    public static RecentLinksWidget Load(IStorageService storageService, string relativeFilePath, string markdown)
    {
        var data = RecentLinksData.FromMarkdown(relativeFilePath, markdown);
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
