using System.Net.Http;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.LinkCollection;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class EditDialog : Page
{
    public EditDialog(
        HttpClient httpClient,
        DispatcherQueue dispatcherQueue,
        IStorageService storageService,
        LinkCollectionWidget widget)
    {
        InitializeComponent();

        ViewModel = new EditViewModel(httpClient, dispatcherQueue, storageService, widget, this);
    }

    public EditViewModel ViewModel { get; }

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
