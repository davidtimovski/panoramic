using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain.RecentLinks;

public class RecentLinksWidget : IWidget
{
    /// <summary>
    /// Constructs a new recent links widget.
    /// </summary>
    public RecentLinksWidget(Area area, string title, int capacity, bool onlyFromToday)
    {
        Id = Guid.NewGuid();
        Type = WidgetType.RecentLinks;
        Area = area;
        Title = title;
        Capacity = capacity;
        OnlyFromToday = onlyFromToday;
        links = [];
    }

    /// <summary>
    /// Constructs a recent links widget based on en existing one.
    /// </summary>
    public RecentLinksWidget(RecentLinksData data)
    {
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

    public static RecentLinksWidget Load(string json, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<RecentLinksData>(json, options)!;
        return new(data);
    }

    public async Task WriteAsync(string storagePath, JsonSerializerOptions options)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");

        var data = GetData();
        var json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(Path.Combine(widgetsDirectory, $"{Id}.json"), json);
    }

    public void Delete(string storagePath)
    {
        var widgetsDirectory = Path.Combine(storagePath, "widgets");
        File.Delete(Path.Combine(widgetsDirectory, $"{Id}.json"));
    }
}

public class RecentLink
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; set; }
}
