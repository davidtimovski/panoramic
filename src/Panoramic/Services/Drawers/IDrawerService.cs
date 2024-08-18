using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Panoramic.Data;
using Panoramic.Services.Drawers.Models;

namespace Panoramic.Services.Drawers;

/// <summary>
/// Handles logic pertaining to link drawers.
/// </summary>
public interface IDrawerService
{
    event EventHandler<LinkDrawersLoadedEventArgs>? LinkDrawersLoaded;

    /// <summary>
    /// Reads link drawers from the file system.
    /// </summary>
    Task ReadLinkDrawersAsync();

    /// <summary>
    /// If the drawer is new, checks whether a file with the same name already exists.
    /// If the drawer is existing but the name has changed, check whether a file with the new name already exists.
    /// </summary>
    /// <param name="name">The name of the drawer.</param>
    /// <param name="originalName">If empty string then the drawer is new.</param>
    /// <returns></returns>
    bool LinkDrawerCanBeCreated(string name, string originalName);

    Task SaveLinkDrawerAsync(LinkDrawerData data, string oldName);

    void DeleteLinkDrawer(string name);

    bool HasDrawers();

    /// <summary>
    /// Searches through the drawers and returns the links ordered by weight and link title.
    /// </summary>
    List<WeighedSearchResult<LinkDrawerLinkData>> SearchDrawers(string searchText);
}
