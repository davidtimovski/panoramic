using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets.RecentLinks;

public sealed partial class RecentLinksSettingsForm : Page, IWidgetForm
{
    public RecentLinksSettingsForm(RecentLinksSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    public RecentLinksSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync();
    }
}
