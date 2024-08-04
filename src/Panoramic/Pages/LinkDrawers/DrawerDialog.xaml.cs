using System.Net.Http;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Data;
using Panoramic.Services.Drawers;
using Panoramic.ViewModels.LinkDrawers;

namespace Panoramic.Pages.LinkDrawers;

public sealed partial class DrawerDialog : Page
{
    public DrawerDialog(
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        IDrawerService drawerService,
        LinkDrawerData? data)
    {
        InitializeComponent();

        ViewModel = new DrawerViewModel(httpClient, dispatcherQueue, drawerService, data, this);
    }

    public DrawerViewModel ViewModel { get; }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.UrlExists())
        {
            DuplicateLinkFlyout.ShowAt(sender as FrameworkElement);
            return;
        }

        ViewModel.Add();
    }

    private void DeleteLinkClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var linkViewModel = (EditLinkViewModel)button.DataContext;
        ViewModel.Delete(linkViewModel);
    }
}
