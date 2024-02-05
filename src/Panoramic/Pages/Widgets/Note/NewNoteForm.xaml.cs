using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NewNoteForm : Page
{
    public NewNoteForm(string directory)
    {
        InitializeComponent();

        ViewModel = new NewNoteViewModel(directory);

        NameTextBox.Loaded += NameTextBox_Loaded;
    }

    public NewNoteViewModel ViewModel { get; }

    private void NameTextBox_Loaded(object _, RoutedEventArgs e)
    {
        NameTextBox.Focus(FocusState.Programmatic);
    }
}
