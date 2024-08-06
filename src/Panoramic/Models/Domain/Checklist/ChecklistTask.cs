using System;

namespace Panoramic.Models.Domain.Checklist;

public sealed class ChecklistTask
{
    public required string Title { get; init; }
    public required DateOnly? DueDate { get; init; }
    public required Uri? Uri { get; init; }
    public required DateTime Created { get; init; }

    /// <summary>
    /// Used for global search functionality.
    /// </summary>
    public bool Matches(string searchText) => Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
        || (Uri is not null && Uri.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase));
}
