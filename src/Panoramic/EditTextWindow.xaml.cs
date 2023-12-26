using Microsoft.UI.Xaml;
using Panoramic.ViewModels;

namespace Panoramic;

public sealed partial class EditTextWindow : Window
{
    public EditTextWindow(EditTextViewModel viewModel)
    {
        this.InitializeComponent();

        Editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, viewModel.Text);

        ViewModel = viewModel;
    }

    public EditTextViewModel ViewModel { get; }
}
