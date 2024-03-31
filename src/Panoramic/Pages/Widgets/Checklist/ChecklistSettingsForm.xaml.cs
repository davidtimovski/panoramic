using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.Checklist;

namespace Panoramic.Pages.Widgets.Checklist;

public sealed partial class ChecklistSettingsForm : Page, IWidgetForm
{
    public ChecklistSettingsForm(ChecklistSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public ChecklistSettingsViewModel ViewModel { get; }

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
