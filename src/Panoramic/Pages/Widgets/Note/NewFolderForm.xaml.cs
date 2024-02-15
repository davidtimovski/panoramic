using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

    private void NameTextBox_Loaded(object _, RoutedEventArgs e)
    {
        NameTextBox.Focus(FocusState.Programmatic);
    }
}
