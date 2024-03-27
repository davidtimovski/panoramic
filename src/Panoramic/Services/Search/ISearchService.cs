using System;

namespace Panoramic.Services.Search;

public interface ISearchService
{
    string SearchText { get; }

    event EventHandler? SearchInvoked;

    void Search(string text);
}
