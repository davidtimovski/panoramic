using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionSettingsForm : Page, IWidgetForm
{
    private readonly string _section;

    public LinkCollectionSettingsForm(string section, LinkCollectionSettingsViewModel viewModel)
    {
        InitializeComponent();

        _section = section;
        ViewModel = viewModel;
    }

    public LinkCollectionSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync(_section);
    }
}
