using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteSettingsForm : Page, IWidgetForm
{
    public NoteSettingsForm(NoteSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        //TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public NoteSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync();
    }

    //private void TitleTextBox_Loaded(object sender, RoutedEventArgs e)
    //{
    //    if (ViewModel.Id == Guid.Empty)
    //    {
    //        TitleTextBox.Focus(FocusState.Programmatic);
    //        TitleTextBox.SelectAll();
    //    }
    //}
}
