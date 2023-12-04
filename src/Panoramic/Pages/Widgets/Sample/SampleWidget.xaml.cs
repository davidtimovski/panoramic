using Microsoft.UI.Xaml.Controls;
using Panoramic.ViewModels.Widgets;

namespace Panoramic.Pages.Widgets.Sample;

public sealed partial class SampleWidget : Page
{
    private readonly string _section;

    public SampleWidget(string section, SampleViewModel viewModel)
    {
        InitializeComponent();

        _section = section;

        ViewModel = viewModel;
    }

    public SampleViewModel ViewModel { get; }
}
