using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets.Checklist;

namespace Panoramic.Pages.Widgets.Checklist;

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
        if (ViewModel.TaskExists())
        {
            DuplicateLinkFlyout.ShowAt(sender as FrameworkElement);
            return;
        }

        ViewModel.Add();
    }

    private void DeleteTaskClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var taskViewModel = (EditTaskViewModel)button.DataContext;
        ViewModel.Delete(taskViewModel);
    }
}
