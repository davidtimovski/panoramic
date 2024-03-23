using System;

namespace Panoramic.Models.Domain.RecentLinks;

public sealed class RecentLink
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; set; }

    /// <summary>
    /// Used for global search functionality.
    /// </summary>
    public bool Matches(string searchText)
    {
        return Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) || Uri.Host.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}
