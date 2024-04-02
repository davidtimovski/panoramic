using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
    public RecentLinksWidget(IStorageService storageService, Area area, string title, int capacity, bool onlyFromToday, bool searchable)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.RecentLinks);

        Area = area;
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
        Title = data.Title;
        Capacity = data.Capacity;
        OnlyFromToday = data.OnlyFromToday;
        Searchable = data.Searchable;
        links = data.Links.Select(x => new RecentLink { Title = x.Title, Uri = x.Uri, Clicked = x.Clicked }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.RecentLinks;
    public Area Area { get; set; }
    public string Title { get; set; }
    public int Capacity { get; set; }
    public bool OnlyFromToday { get; set; }
    public bool Searchable { get; set; }

    private List<RecentLink> links;
    public IReadOnlyList<RecentLink> Links
    {
        get => links.OrderByDescending(x => x.Clicked).Take(Capacity).ToList();
        private set
        {
            links = [.. value];
        }
    }

    public void HyperlinkClicked(string title, Uri uri, DateTime clicked)
    {
        var clickedLink = new RecentLink { Title = title, Uri = uri, Clicked = clicked };

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
            links[links.IndexOf(link)] = clickedLink;
        }

        var query = Links.AsEnumerable();
        if (OnlyFromToday)
        {
            query = query.Where(x => x.Clicked >= DateTime.Today);
        }

        links.Clear();
        links.AddRange(query.OrderByDescending(x => x.Clicked).Take(Capacity));
    }

    public void Clear() => links.Clear();

    public RecentLinksData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            Title = Title,
            Capacity = Capacity,
            OnlyFromToday = OnlyFromToday,
            Searchable = Searchable,
            Links = Links.Select(x => new RecentLinkData { Title = x.Title, Uri = x.Uri, Clicked = x.Clicked }).ToList()
        };

    public static RecentLinksWidget Load(IStorageService storageService, string json)
    {
        var data = JsonSerializer.Deserialize<RecentLinksData>(json, storageService.SerializerOptions)!;
        return new(storageService, data);
    }

    public async Task WriteAsync()
    {
        DebugLogger.Log($"Writing {Type} widget with ID: {Id}");

        var data = GetData();
        var json = JsonSerializer.Serialize(data, _storageService.SerializerOptions);

        await File.WriteAllTextAsync(Path.Combine(_storageService.WidgetsFolderPath, _dataFileName), json);
    }

    public void Delete()
    {
        var dataFilePath = Path.Combine(_storageService.WidgetsFolderPath, _dataFileName);
        File.Delete(dataFilePath);
    }
}
