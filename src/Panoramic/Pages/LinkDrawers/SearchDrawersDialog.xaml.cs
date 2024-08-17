using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Panoramic.Services;
using Panoramic.Services.Drawers;
using Panoramic.ViewModels.LinkDrawers;

namespace Panoramic.Pages.LinkDrawers;

public sealed partial class SearchDrawersDialog : Page
{
    public SearchDrawersDialog(IDrawerService drawerService, IEventHub eventHub)
    {
        InitializeComponent();

        ViewModel = new SearchDrawersViewModel(drawerService, eventHub, this);
    }

    public SearchDrawersViewModel ViewModel { get; }

    private void DownHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.SelectNextLink();
        args.Handled = true;
    }

    private void UpHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.SelectPreviousLink();
        args.Handled = true;
    }

    private void EnterHotkey_Invoked(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NavigateToCurrentLink();
        args.Handled = true;
    }
}
