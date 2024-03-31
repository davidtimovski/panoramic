using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NewFolderForm : Page
{
    public NewFolderForm(string directory)
    {
        InitializeComponent();

        ViewModel = new NewFolderViewModel(directory);

        NameTextBox.Loaded += NameTextBox_Loaded;
    }

    public NewFolderViewModel ViewModel { get; }

    public event EventHandler<EventArgs>? Submitted;

    private void NameTextBox_Loaded(object _, RoutedEventArgs e)
    {
        NameTextBox.Focus(FocusState.Programmatic);
    }

    private void TextBoxEnter_Pressed(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (ViewModel.CanBeCreated())
        {
            Submitted?.Invoke(this, EventArgs.Empty);
        }
    }
}
