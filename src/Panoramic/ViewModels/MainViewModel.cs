using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Panoramic.Services.Search;

namespace Panoramic.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly TimeSpan SearchTextChangeDebounceInterval = TimeSpan.FromMilliseconds(250);

    private readonly DispatcherQueueTimer _debounceTimer;
    private readonly ISearchService _searchService;

    public MainViewModel(ISearchService searchService)
    {
        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;
        _debounceTimer = queue.CreateTimer();

        _searchService = searchService;
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
