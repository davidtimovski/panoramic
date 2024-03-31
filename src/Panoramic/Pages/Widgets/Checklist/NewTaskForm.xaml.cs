using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.Models.Domain.Checklist;
using Panoramic.ViewModels.Widgets.Checklist;

namespace Panoramic.Pages.Widgets.Checklist;

public sealed partial class NewTaskForm : Page
{
    public NewTaskForm(ChecklistWidget widget)
    {
        InitializeComponent();

        ViewModel = new NewTaskViewModel(widget);

        TitleTextBox.Loaded += TitleTextBox_Loaded;
    }

    public NewTaskViewModel ViewModel { get; }

    public event EventHandler<EventArgs>? Submitted;

    private void TitleTextBox_Loaded(object _, RoutedEventArgs e)
    {
        TitleTextBox.Focus(FocusState.Programmatic);
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
