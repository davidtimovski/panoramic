using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Panoramic.Services.Search;

namespace Panoramic.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly TimeSpan SearchTextChangeDebounceInterval = TimeSpan.FromMilliseconds(250);

    private readonly ISearchService _searchService;
    private readonly DispatcherQueueTimer _debounceTimer;

    public MainViewModel(DispatcherQueue dispatcherQueue, ISearchService searchService)
    {
        _searchService = searchService;
        _debounceTimer = dispatcherQueue.CreateTimer();
    }

    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value))
            {
                return;
            }

            OnPropertyChanged();

            var trimmed = value.Trim();
            _debounceTimer.Debounce(() => _searchService.Search(trimmed), SearchTextChangeDebounceInterval);
        }
    }
}
