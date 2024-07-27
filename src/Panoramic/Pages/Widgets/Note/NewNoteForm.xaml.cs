using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.Services.Notes.Models;
using Panoramic.Services.Storage.Models;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NewNoteForm : Page
{
    public NewNoteForm(IReadOnlyList<FileSystemItem> fileSystemItems, FileSystemItemPath path, string storagePath)
    {
        InitializeComponent();

        ViewModel = new NewNoteViewModel(fileSystemItems, path, storagePath);

        NameTextBox.Loaded += NameTextBox_Loaded;
    }

    public NewNoteViewModel ViewModel { get; }

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
