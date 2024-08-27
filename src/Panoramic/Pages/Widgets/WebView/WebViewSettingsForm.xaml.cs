using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.WebView;

namespace Panoramic.Pages.Widgets.WebView;

public sealed partial class WebViewSettingsForm : Page, IWidgetForm
{
    public WebViewSettingsForm(WebViewSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public WebViewSettingsViewModel ViewModel { get; }

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
