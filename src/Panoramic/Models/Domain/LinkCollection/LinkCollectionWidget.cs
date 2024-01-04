﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panoramic.Models.Domain.LinkCollection;

public class LinkCollectionWidget : IWidget
{
    /// <summary>
    /// Constructs a new link collection widget.
    /// </summary>
    public LinkCollectionWidget(Area area, string title)
    {
        Id = Guid.NewGuid();
        Type = WidgetType.LinkCollection;
        Area = area;
        Title = title;
        links = [];
    }

    /// <summary>
    /// Constructs a link collection widget based on en existing one.
    /// </summary>
    public LinkCollectionWidget(LinkCollectionData data)
    {
        Id = data.Id;
        Type = WidgetType.LinkCollection;
        Area = data.Area;
        Title = data.Title;
        links = data.Links.Select(x => new LinkCollectionItem { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; }
    public Area Area { get; set; }
    public string Title { get; set; }

    private List<LinkCollectionItem> links;
    public IReadOnlyList<LinkCollectionItem> Links
    {
        get
        {
            return links.OrderBy(x => x.Order).ToList();
        }
        private set
        {
            links = [.. value];
        }
    }

    public void SetData(List<LinkCollectionItem> links)
    {
        this.links = links;
    }

    public LinkCollectionData GetData() =>
        new()
        {
            Id = Id,
            Type = WidgetType.LinkCollection,
            Area = Area,
            Title = Title,
            Links = links.Select(x => new LinkCollectionItemData { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList()
        };

    public static LinkCollectionWidget Load(string json, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<LinkCollectionData>(json, options)!;
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

public class LinkCollectionItem
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }
}
