using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteRenameForm : Page
{
    public NoteRenameForm(string path)
    {
        InitializeComponent();

        ViewModel = new NoteRenameViewModel(path);

        NameTextBox.Loaded += NameTextBox_Loaded;
    }

    public NoteRenameViewModel ViewModel { get; }

    private void NameTextBox_Loaded(object _, RoutedEventArgs e)
    {
        NameTextBox.Focus(FocusState.Programmatic);
        NameTextBox.SelectAll();
    }
}
