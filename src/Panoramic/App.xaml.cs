using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Panoramic.Services;
using Panoramic.Services.Storage;
using Panoramic.ViewModels;

namespace Panoramic;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        InitializeComponent();

        ServiceCollection services = new();

        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton(new HttpClient());

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<IEventHub, EventHub>();
        services.AddSingleton<IMarkdownService, MarkdownService>();

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
