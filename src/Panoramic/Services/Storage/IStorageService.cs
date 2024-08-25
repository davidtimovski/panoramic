using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Panoramic.Models.Domain;
using Panoramic.Services.Storage.Models;

namespace Panoramic.Services.Storage;

/// <summary>
/// Handles general storage and widgets.
/// </summary>
public interface IStorageService
{
    event EventHandler<WidgetUpdatedEventArgs>? WidgetUpdated;
    event EventHandler<WidgetDeletedEventArgs>? WidgetDeleted;
    event EventHandler<EventArgs>? StoragePathChanged;

    string StoragePath { get; }
    string SystemFolderPath { get; }
    string WidgetsFolderPath { get; }
    Dictionary<Guid, IWidget> Widgets { get; }

    /// <summary>
    /// Reads widgets from the file system.
    /// </summary>
    /// <param name="noteWidgets">Note widgets loaded earlier.</param>
    Task ReadWidgetsAsync(IReadOnlyList<IWidget> noteWidgets);
    Task WriteUnsavedChangesAsync();

    /// <summary>
    /// Schedules a widget save to disk.
    /// Will reset the auto-save timer if other widget changes have been enqueued (but only if the first enqueued change is earlier than 1 minute ago).
    /// </summary>
    void EnqueueWidgetWrite(Guid id, string change);

    void DeleteWidget(IWidget widget);
    Task AddNewWidgetAsync(IWidget widget);
    Task SaveWidgetAsync(IWidget widget);

    void ChangeStoragePath(string storagePath);
}
