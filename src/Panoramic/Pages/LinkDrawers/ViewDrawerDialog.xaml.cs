using Microsoft.UI.Xaml.Controls;
using Panoramic.Data;
using Panoramic.Services;
using Panoramic.ViewModels.LinkDrawers;

namespace Panoramic.Pages.LinkDrawers;

public sealed partial class ViewDrawerDialog : Page
{
    public ViewDrawerDialog(IEventHub eventHub, LinkDrawerData data)
    {
        InitializeComponent();

        ViewModel = new ViewDrawerViewModel(eventHub, data);
    }

    public ViewDrawerViewModel ViewModel { get; }
}
