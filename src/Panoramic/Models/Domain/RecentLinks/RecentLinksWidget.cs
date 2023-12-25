using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Panoramic.Models.Domain.RecentLinks;

public class RecentLinksWidget : Widget
{
    public const string DefaultTitle = "Recent";
    public const int DefaultCapacity = 15;
    public const bool DefaultOnlyFromToday = false;

    /// <summary>
    /// Constructs a new recent links widget.
    /// </summary>
    public RecentLinksWidget(Area area, string title, int capacity, bool onlyFromToday)
        : base(Guid.NewGuid(), WidgetType.RecentLinks, area, title)
    {
        Capacity = capacity;
        OnlyFromToday = onlyFromToday;
        links = new();
    }

    /// <summary>
    /// Constructs a recent links widget based on en existing one.
    /// </summary>
    public RecentLinksWidget(RecentLinksData data)
        : base(data.Id, WidgetType.RecentLinks, data.Area, data.Title)
    {
        Capacity = data.Capacity;
        OnlyFromToday = data.OnlyFromToday;
        links = data.Links.Select(x => new RecentLink { Title = x.Title, Uri = x.Uri, Clicked = x.Clicked }).ToList();
    }

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
            links = value.ToList();
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

    public string Serialize(JsonSerializerOptions options)
    {
        var data = GetData();
        return JsonSerializer.Serialize(data, options);
    }

    public static RecentLinksWidget Load(string json, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<RecentLinksData>(json, options)!;
        return new(data);
    }
}

public class RecentLink
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; set; }
}
