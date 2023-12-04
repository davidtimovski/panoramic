using Microsoft.UI.Xaml.Controls;

namespace Panoramic.UserControls;

public sealed partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
    }

    public string Title { get; set; } = null!;
}
