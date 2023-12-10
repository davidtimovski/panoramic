using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.LinkCollection;

namespace Panoramic.Pages.Widgets.LinkCollection;

public sealed partial class LinkCollectionSettingsForm : Page, IWidgetForm
{
    public LinkCollectionSettingsForm(LinkCollectionSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    public LinkCollectionSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync();
    }
}
