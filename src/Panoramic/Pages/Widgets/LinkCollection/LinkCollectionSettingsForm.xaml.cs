using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionSettingsForm : Page, IWidgetForm
{
    public LinkCollectionSettingsForm(LinkCollectionSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public LinkCollectionSettingsViewModel ViewModel { get; }

    public Task SubmitAsync() => ViewModel.SubmitAsync();

    private void TitleTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Id == Guid.Empty)
        {
            TitleTextBox.Focus(FocusState.Programmatic);
            TitleTextBox.SelectAll();
        }
    }
}
