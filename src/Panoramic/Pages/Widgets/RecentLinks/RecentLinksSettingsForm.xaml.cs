using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets.RecentLinks;

public sealed partial class RecentLinksSettingsForm : Page, IWidgetForm
{
    public RecentLinksSettingsForm(RecentLinksSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public RecentLinksSettingsViewModel ViewModel { get; }

    public Task SubmitAsync() => ViewModel.SubmitAsync();

    private void TitleTextBox_Loaded(object _, RoutedEventArgs e)
    {
        if (ViewModel.Id == Guid.Empty)
        {
            TitleTextBox.Focus(FocusState.Programmatic);
            TitleTextBox.SelectionStart = TitleTextBox.Text.Length;
        }
    }
}
