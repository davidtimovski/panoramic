using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class EditDialog : Page
{
    public EditDialog(EditViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    public EditViewModel ViewModel { get; }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.LinkExists())
        {
            AddLinkButton.Flyout.ShowAt(sender as FrameworkElement);
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
