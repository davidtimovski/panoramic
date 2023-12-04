using CommunityToolkit.Mvvm.ComponentModel;

namespace Panoramic.ViewModels;

public partial class AddLinkViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string uri = string.Empty;
}
