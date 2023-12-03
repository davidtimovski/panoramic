using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets;

namespace Panoramic.Pages.Widgets;

public sealed partial class SampleWidget : Page
{
    public SampleWidget(SampleViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    public SampleViewModel ViewModel { get; }
}
