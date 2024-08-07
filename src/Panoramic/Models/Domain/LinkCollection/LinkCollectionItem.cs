﻿using System;

namespace Panoramic.Models.Domain.LinkCollection;

public sealed class LinkCollectionItem
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required short Order { get; init; }

    /// <summary>
    /// Used for global search functionality.
    /// </summary>
    public bool Matches(string searchText)
        => Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) || Uri.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase);
}
