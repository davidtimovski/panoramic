using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.Pages;
using Panoramic.Pages.Widgets;
using Panoramic.Pages.Widgets.RecentLinks;
using Panoramic.Pages.Widgets.Sample;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic;

public sealed partial class MainWindow : Window
{
    private readonly IStorageService _storageService;
    private readonly IEventHub _eventHub;
    private readonly HttpClient _httpClient;
    private readonly DispatcherQueue _dispatcherQueue;

    public MainWindow(IStorageService storageService, IEventHub eventHub, HttpClient httpClient, DispatcherQueue dispatcherQueue, MainViewModel viewModel)
    {
        InitializeComponent();

        Closed += WindowClosed;

        _storageService = storageService;
        _storageService.WidgetUpdated += WidgetUpdated;
        _storageService.WidgetRemoved += WidgetRemoved;

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
        await _storageService.WriteAsync();
    }

    // TODO: Ugly, find way to make generic
    private void SetFrames()
    {
        if (_storageService.Sections.TryGetValue("A1", out var a1Data))
        {
            FrameA1.Content = GetFrameContent("A1", a1Data!);
        }

        if (_storageService.Sections.TryGetValue("A2", out var a2Data))
        {
            FrameA2.Content = GetFrameContent("A2", a2Data!);
        }

        if (_storageService.Sections.TryGetValue("A3", out var a3Data))
        {
            FrameA3.Content = GetFrameContent("A3", a3Data!);
        }

        if (_storageService.Sections.TryGetValue("B1", out var b1Data))
        {
            FrameB1.Content = GetFrameContent("B1", b1Data!);
        }

        if (_storageService.Sections.TryGetValue("B2", out var b2Data))
        {
            FrameB2.Content = GetFrameContent("B2", b2Data!);
        }

        if (_storageService.Sections.TryGetValue("B3", out var b3Data))
        {
            FrameB3.Content = GetFrameContent("B3", b3Data!);
        }

        if (_storageService.Sections.TryGetValue("C1", out var c1Data))
        {
            FrameC1.Content = GetFrameContent("C1", c1Data!);
        }

        if (_storageService.Sections.TryGetValue("C2", out var c2Data))
        {
            FrameC2.Content = GetFrameContent("C2", c2Data!);
        }

        if (_storageService.Sections.TryGetValue("C3", out var c3Data))
        {
            FrameC3.Content = GetFrameContent("C3", c3Data!);
        }

        if (_storageService.Sections.TryGetValue("D1", out var d1Data))
        {
            FrameD1.Content = GetFrameContent("D1", d1Data!);
        }

        if (_storageService.Sections.TryGetValue("D2", out var d2Data))
        {
            FrameD2.Content = GetFrameContent("D2", d2Data!);
        }
    }

    private object GetFrameContent(string section, WidgetData data)
    {
        return data.Type switch
        {
            WidgetType.Sample => new SampleWidget(section, new SampleViewModel((SampleWidgetData)data)),
            WidgetType.RecentLinks => new RecentLinksWidget(section, _storageService, new RecentLinksViewModel(_eventHub, (RecentLinksWidgetData)data)),
            _ => throw new InvalidOperationException("Unsupported widget type")
        };
    }

    private async void AddWidgetButton_Click(object _, RoutedEventArgs e)
    {
        var content = new AddWidgetDialog(_storageService);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = AddWidgetDialog.SectionPickerTitle,
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync),
            IsPrimaryButtonEnabled = false
        };

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.SubmitEnabledChanged += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Enabled; };

        await dialog.ShowAsync();
    }

    // TODO: Ugly, find way to make generic
    private void WidgetUpdated(object? _, WidgetUpdatedEventArgs e)
    {
        var data = _storageService.Sections[e.Section];

        switch (e.Section)
        {
            case "A1":
                FrameA1.Content = GetFrameContent("A1", data!);
                break;
            case "B1":
                FrameB1.Content = GetFrameContent("B1", data!);
                break;
            case "C1":
                FrameC1.Content = GetFrameContent("C1", data!);
                break;
            case "A2":
                FrameA2.Content = GetFrameContent("A2", data!);
                break;
            case "B2":
                FrameB2.Content = GetFrameContent("B2", data!);
                break;
            case "C2":
                FrameC2.Content = GetFrameContent("C2", data!);
                break;
            case "A3":
                FrameA3.Content = GetFrameContent("A3", data!);
                break;
            case "B3":
                FrameB3.Content = GetFrameContent("B3", data!);
                break;
            case "C3":
                FrameC3.Content = GetFrameContent("C3", data!);
                break;
            case "D1":
                FrameD1.Content = GetFrameContent("D1", data!);
                break;
            case "D2":
                FrameD2.Content = GetFrameContent("D2", data!);
                break;
        }
    }
    
    // TODO: Ugly, find way to make generic
    private void WidgetRemoved(object? _, WidgetRemovedEventArgs e)
    {
        switch (e.Section)
        {
            case "A1":
                FrameA1.Content = null;
                break;
            case "B1":
                FrameB1.Content = null;
                break;
            case "C1":
                FrameC1.Content = null;
                break;
            case "A2":
                FrameA2.Content = null;
                break;
            case "B2":
                FrameB2.Content = null;
                break;
            case "C2":
                FrameC2.Content = null;
                break;
            case "A3":
                FrameA3.Content = null;
                break;
            case "B3":
                FrameB3.Content = null;
                break;
            case "C3":
                FrameC3.Content = null;
                break;
            case "D1":
                FrameD1.Content = null;
                break;
            case "D2":
                FrameD2.Content = null;
                break;
        }
    }
}
