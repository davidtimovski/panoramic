using Microsoft.UI.Xaml.Controls;
using Panoramic.Services;
using Panoramic.Services.Drawers;
using Panoramic.ViewModels.LinkDrawers;

namespace Panoramic.Pages.LinkDrawers;

public sealed partial class SearchDrawersDialog : Page
{
    public SearchDrawersDialog(IDrawerService drawerService, IEventHub eventHub)
    {
        InitializeComponent();

        ViewModel = new SearchDrawersViewModel(drawerService, eventHub);
    }

    public SearchDrawersViewModel ViewModel { get; }
}
