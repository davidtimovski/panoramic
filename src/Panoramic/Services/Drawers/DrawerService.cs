using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Services.Drawers.Models;
using Panoramic.Services.Storage;

namespace Panoramic.Services.Drawers;

/// <inheritdoc/>
public sealed class DrawerService : IDrawerService
{
    private readonly IStorageService _storageService;
    private readonly string _linkDrawersFolderPath;

    private readonly Dictionary<string, LinkDrawerData> _drawers = [];

    public DrawerService(IStorageService storageService)
    {
        _storageService = storageService;
        _linkDrawersFolderPath = Path.Combine(_storageService.SystemFolderPath, "link-drawers");

        if (!Directory.Exists(_linkDrawersFolderPath))
        {
            Directory.CreateDirectory(_linkDrawersFolderPath);
        }
    }

    public event EventHandler<LinkDrawersLoadedEventArgs>? LinkDrawersLoaded;

    /// <inheritdoc/>
    public async Task ReadLinkDrawersAsync()
    {
        var drawerFilePaths = Directory.GetFiles(_linkDrawersFolderPath, "*.md");

        var tasks = drawerFilePaths.Select(ReadLinkDrawerAsync);

        var drawers = await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (var drawer in drawers)
        {
            _drawers.Add(drawer.Name, drawer);
        }

        LinkDrawersLoaded?.Invoke(this, new LinkDrawersLoadedEventArgs { Drawers = drawers });
    }

    /// <inheritdoc/>
    public bool LinkDrawerCanBeCreated(string name, string originalName)
    {
        bool FileDoesNotExist(string name)
        {
            var path = Path.Combine(_linkDrawersFolderPath, $"{name}.md");
            return !File.Exists(path);
        }

        var isNew = originalName == string.Empty;

        if (isNew)
        {
            return FileDoesNotExist(name);
        }
        else if (!string.Equals(name, originalName, StringComparison.Ordinal))
        {
            return FileDoesNotExist(name);
        }

        return true;
    }

    public async Task SaveLinkDrawerAsync(LinkDrawerData data, string oldName)
    {
        var builder = new StringBuilder();
        data.ToMarkdown(builder);

        await File.WriteAllTextAsync(Path.Combine(_linkDrawersFolderPath, $"{data.Name}.md"), builder.ToString());
        if (!_drawers.TryAdd(data.Name, data))
        {
            _drawers[data.Name] = data;
        }

        if (oldName != string.Empty && !string.Equals(data.Name, oldName, StringComparison.Ordinal))
        {
            File.Delete(Path.Combine(_linkDrawersFolderPath, $"{oldName}.md"));
            _drawers.Remove(oldName);
        }

        LinkDrawersLoaded?.Invoke(this, new LinkDrawersLoadedEventArgs { Drawers = _drawers.Values });
    }

    public void DeleteLinkDrawer(string name)
    {
        File.Delete(Path.Combine(_linkDrawersFolderPath, $"{name}.md"));
        _drawers.Remove(name);

        LinkDrawersLoaded?.Invoke(this, new LinkDrawersLoadedEventArgs { Drawers = _drawers.Values });
    }

    public bool HasDrawers() => _drawers.Count > 0;

    /// <inheritdoc/>
    public List<WeighedSearchResult<LinkDrawerLinkData>> SearchDrawers(string searchText)
    {
        var result = new List<WeighedSearchResult<LinkDrawerLinkData>>();

        foreach (var drawer in _drawers.Values)
        {
            var matched = drawer.Links.Select(x => x.Matches(searchText, drawer.Name));
            result.AddRange(matched);
        }

        return result.Where(x => x.Weight > 0)
            .OrderByDescending(x => x.Weight)
            .ThenBy(x => x.Result.Title)
            .ToList();
    }

    private async Task<LinkDrawerData> ReadLinkDrawerAsync(string fileName)
    {
        var relativeFilePath = Path.GetRelativePath(_storageService.StoragePath, fileName);

        var markdown = await File.ReadAllTextAsync(Path.Combine(_linkDrawersFolderPath, fileName));
        return LinkDrawerData.FromMarkdown(relativeFilePath, markdown);
    }
}
