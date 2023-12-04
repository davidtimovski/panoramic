using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;
using Panoramic.ViewModels.RecentLinks;

namespace Panoramic.Pages.Widgets.RecentLinks;

public sealed partial class RecentLinksSettingsForm : Page, IWidgetForm
{
    private readonly string _section;

    public RecentLinksSettingsForm(string section, RecentLinksSettingsViewModel viewModel)
    {
        InitializeComponent();

        _section = section;
        ViewModel = viewModel;
    }

    public RecentLinksSettingsViewModel ViewModel { get; }

    public Task SubmitAsync()
    {
        return ViewModel.SubmitAsync(_section);
    }
}
