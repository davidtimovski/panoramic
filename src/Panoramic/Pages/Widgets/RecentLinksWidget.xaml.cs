using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets.RecentLinks;

namespace Panoramic.Pages.Widgets;

public sealed partial class RecentLinksWidget : Page
{
    public RecentLinksWidget(RecentLinksViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    public RecentLinksViewModel ViewModel { get; }
}
