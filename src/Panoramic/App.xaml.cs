using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Panoramic.Services;
using Panoramic.Services.Markdown;
using Panoramic.Services.Preferences;
using Panoramic.Services.Search;
using Panoramic.Services.Storage;
using Panoramic.ViewModels;
using Windows.Storage;

namespace Panoramic;

public sealed partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        InitializeComponent();

        InitializeTheme();

        ServiceCollection services = [];

        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IPreferencesService, PreferencesService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton(new HttpClient());

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<IEventHub, EventHub>();
        services.AddSingleton<IMarkdownService, MarkdownService>();
        services.AddSingleton<ISearchService, SearchService>();

        services.AddSingleton(DispatcherQueue.GetForCurrentThread());
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var isNewInstance = await CheckAppInstanceAsync();
        if (!isNewInstance)
        {
            // Exit our instance and stop
            Process.GetCurrentProcess().Kill();
            return;
        }

        var storageService = _serviceProvider.GetRequiredService<IStorageService>();
        await storageService.ReadAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }

    private static void InitializeTheme()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        object? themeValue = localSettings.Values[nameof(IPreferencesService.Theme)];

        if (themeValue is null)
        {
            return;
        }

        Current.RequestedTheme = (ApplicationTheme)themeValue;
    }

    /// <summary>
    /// Helps make sure that we have only a single instance of the app running.
    /// </summary>
    private static async Task<bool> CheckAppInstanceAsync()
    {
        var appArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        // Get or register the main instance
        var mainInstance = AppInstance.FindOrRegisterForKey("main");

        // If the main instance isn't this current instance
        if (!mainInstance.IsCurrent)
        {
            // Redirect activation to that instance
            await mainInstance.RedirectActivationToAsync(appArgs);

            return false;
        }

        return true;
    }
}
