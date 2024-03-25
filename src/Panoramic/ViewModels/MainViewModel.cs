using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Panoramic.Services;

namespace Panoramic.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly TimeSpan SearchTextChangeDebounceInterval = TimeSpan.FromMilliseconds(200);

    private readonly DispatcherQueueTimer _debounceTimer;
    private readonly IEventHub _eventHub;

    public MainViewModel(IEventHub eventHub)
    {
        var queueController = DispatcherQueueController.CreateOnDedicatedThread();
        var queue = queueController.DispatcherQueue;
        _debounceTimer = queue.CreateTimer();

        _eventHub = eventHub;
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
            if (trimmed.Length > 0)
            {
                _debounceTimer.Debounce(() => _eventHub.RaiseSearchInvoked(trimmed), SearchTextChangeDebounceInterval);
            }
            else
            {
                _eventHub.RaiseSearchInvoked(trimmed);
            }
        }
    }
}
