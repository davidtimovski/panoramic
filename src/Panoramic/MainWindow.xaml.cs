using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Pages;
using Panoramic.Pages.Widgets;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic;

public sealed partial class MainWindow : Window
{
    private readonly IEventHub _eventHub;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;

    public MainWindow(IEventHub eventHub, HttpClient httpClient, DispatcherQueue dispatcherQueue, MainViewModel viewModel)
    {
        InitializeComponent();

        Closed += WindowClosed;

        _eventHub = eventHub;
        _httpClient = httpClient;
        _dispatcherQueue = dispatcherQueue;
        
        ViewModel = viewModel;

        SetFrames();
    }

    public MainViewModel ViewModel { get; }

    private async void AddBookmarkButton_Click(object _, RoutedEventArgs e)
    {
        var content = new AddBookmarkDialog(_httpClient, _dispatcherQueue, new AddBookmarkViewModel());
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Add bookmark",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => ViewModel.AddBookmark(content.ViewModel.Title, content.ViewModel.Uri))
        };
        await dialog.ShowAsync();
    }

    private async void WindowClosed(object _, WindowEventArgs args)
    {
        await StorageService.WriteAsync();
    }

    private void SetFrames()
    {
        if (StorageService.Frames.TryGetValue("A1", out var a1Data))
        {
            FrameA1.Content = GetFrameContent(a1Data!);
        }

        if (StorageService.Frames.TryGetValue("A2", out var a2Data))
        {
            FrameA2.Content = GetFrameContent(a2Data!);
        }

        if (StorageService.Frames.TryGetValue("A3", out var a3Data))
        {
            FrameA3.Content = GetFrameContent(a3Data!);
        }

        if (StorageService.Frames.TryGetValue("B1", out var b1Data))
        {
            FrameB1.Content = GetFrameContent(b1Data!);
        }

        if (StorageService.Frames.TryGetValue("B2", out var b2Data))
        {
            FrameB2.Content = GetFrameContent(b2Data!);
        }

        if (StorageService.Frames.TryGetValue("B3", out var b3Data))
        {
            FrameB3.Content = GetFrameContent(b3Data!);
        }

        if (StorageService.Frames.TryGetValue("C1", out var c1Data))
        {
            FrameC1.Content = GetFrameContent(c1Data!);
        }

        if (StorageService.Frames.TryGetValue("C2", out var c2Data))
        {
            FrameC2.Content = GetFrameContent(c2Data!);
        }

        if (StorageService.Frames.TryGetValue("C3", out var c3Data))
        {
            FrameC3.Content = GetFrameContent(c3Data!);
        }

        if (StorageService.Frames.TryGetValue("D1", out var d1Data))
        {
            FrameD1.Content = GetFrameContent(d1Data!);
        }

        if (StorageService.Frames.TryGetValue("D2", out var d2Data))
        {
            FrameD2.Content = GetFrameContent(d2Data!);
        }
    }

    private object GetFrameContent(WidgetData data)
    {
        return data.Type switch
        {
            WidgetType.Sample => new SampleWidget(new SampleViewModel((SampleWidgetData)data)),
            WidgetType.RecentLinks => new RecentLinksWidget(new RecentLinksViewModel(_eventHub, (RecentLinksWidgetData)data)),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };
    }
}
