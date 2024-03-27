using System;

namespace Panoramic.Services.Search;

public sealed class SearchService : ISearchService
{
    public string SearchText { get; private set; } = string.Empty;

    public event EventHandler? SearchInvoked;

    public void Search(string text)
    {
        SearchText = text;
        SearchInvoked?.Invoke(this, EventArgs.Empty);
    }
}
