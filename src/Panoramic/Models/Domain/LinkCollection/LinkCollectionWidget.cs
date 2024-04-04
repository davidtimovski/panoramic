using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Services.Storage;
using Panoramic.Utils;

namespace Panoramic.Models.Domain.LinkCollection;

public sealed class LinkCollectionWidget : IWidget
{
    private readonly IStorageService _storageService;
    private readonly string _dataFileName;

    /// <summary>
    /// Constructs a new link collection widget.
    /// </summary>
    public LinkCollectionWidget(IStorageService storageService, Area area, string title, bool searchable)
    {
        _storageService = storageService;

        Id = Guid.NewGuid();
        _dataFileName = WidgetUtil.CreateDataFileName(Id, WidgetType.LinkCollection);

        Area = area;
        Title = title;
        Searchable = searchable;
        links = [];
    }

    /// <summary>
    /// Constructs a link collection widget based on existing data.
    /// </summary>
    private LinkCollectionWidget(IStorageService storageService, LinkCollectionData data)
    {
        _storageService = storageService;
        _dataFileName = WidgetUtil.CreateDataFileName(data.Id, WidgetType.LinkCollection);

        Id = data.Id;
        Area = data.Area;
        Title = data.Title;
        Searchable = data.Searchable;
        links = data.Links.Select(x => new LinkCollectionItem { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList();
    }

    public Guid Id { get; }
    public WidgetType Type { get; } = WidgetType.LinkCollection;
    public Area Area { get; set; }
    public string Title { get; set; }
    public bool Searchable { get; set; }

    private List<LinkCollectionItem> links;
    public IReadOnlyList<LinkCollectionItem> Links
    {
        get => links;
        set
        {
            links = [.. value.OrderBy(x => x.Order)];
        }
    }

    public LinkCollectionData GetData() =>
        new()
        {
            Id = Id,
            Area = Area,
            Title = Title,
            Searchable = Searchable,
            Links = links.Select(x => new LinkCollectionItemData { Title = x.Title, Uri = x.Uri, Order = x.Order }).ToList()
        };

    public static LinkCollectionWidget Load(IStorageService storageService, string json)
    {
        var data = JsonSerializer.Deserialize<LinkCollectionData>(json, storageService.SerializerOptions)!;
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
