using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Services.Storage;

namespace Panoramic.Models.Domain.RecentLinks;

public class RecentLinksWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new recent links widget.
    /// </summary>
    public RecentLinksWidget(IStorageService storageService, Area area, string title, int capacity, bool onlyFromToday)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = $"{Id}.json";

        Type = WidgetType.RecentLinks;
        Area = area;
        Title = title;
        Capacity = capacity;
        OnlyFromToday = onlyFromToday;
        links = [];
    }

    /// <summary>
    /// Constructs a recent links widget based on existing data.
    /// </summary>
    public RecentLinksWidget(IStorageService storageService, RecentLinksData data)
    {
        _storageService = storageService;
        _dataFileName = $"{data.Id}.json";

        Id = data.Id;
        Type = WidgetType.RecentLinks;
        Area = data.Area;
        Title = data.Title;
        Capacity = data.Capacity;
        OnlyFromToday = data.OnlyFromToday;
        links = data.Links.Select(x => new RecentLink { Title = x.Title, Uri = x.Uri, Clicked = x.Clicked }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public string Title { get; set; }
    public int Capacity { get; set; }
    public bool OnlyFromToday { get; set; }

    private List<RecentLink> links;
    public IReadOnlyList<RecentLink> Links
    {
        get
        {
            return links.OrderByDescending(x => x.Clicked).ToList();
        }
        private set
        {
            links = [.. value];
        }
    }

    public void HyperlinkClicked(string title, Uri uri, DateTime clicked)
    {
        var link = Links.FirstOrDefault(x => string.Equals(x.Uri.ToString(), uri.ToString(), StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            links.Add(new RecentLink { Title = title, Uri = uri, Clicked = clicked });

            if (Links.Count > Capacity)
            {
                links.RemoveAt(0);
            }
        }
        else
        {
            link.Clicked = clicked;
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
            Type = WidgetType.RecentLinks,
            Area = Area,
            Title = Title,
            Capacity = Capacity,
            OnlyFromToday = OnlyFromToday,
            Links = Links.Select(x => new RecentLinkData { Title = x.Title, Uri = x.Uri, Clicked = x.Clicked }).ToList()
        };

    public static RecentLinksWidget Load(IStorageService storageService, string json)
    {
        var data = JsonSerializer.Deserialize<RecentLinksData>(json, storageService.SerializerOptions)!;
        return new(storageService, data);
    }

    public async Task WriteAsync()
    {
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

public class RecentLink
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; set; }
}
