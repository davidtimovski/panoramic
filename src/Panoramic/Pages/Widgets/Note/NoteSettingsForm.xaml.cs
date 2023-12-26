using System.Threading.Tasks;
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
    }

    public NoteSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync();
    }
}
