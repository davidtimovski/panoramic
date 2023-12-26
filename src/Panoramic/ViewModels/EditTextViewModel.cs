using CommunityToolkit.Mvvm.ComponentModel;

namespace Panoramic.ViewModels;

public partial class EditTextViewModel : ObservableObject
{
    public EditTextViewModel(string title, string text)
    {
        this.title = title;
        this.text = text;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string text;
}
