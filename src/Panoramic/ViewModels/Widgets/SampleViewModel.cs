using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets;

public partial class SampleViewModel : ObservableObject
{
    public SampleViewModel(SampleWidgetData data)
    {
        Title = data.Title;
        Text = data.Text;
    }

    [ObservableProperty]
    private string title;

    public string Text { get; set; }
}
