using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(string defaultTitle, WidgetData? data)
    {
        title = data is null ? defaultTitle : data.Title;
    }

    [ObservableProperty]
    private string title;

    protected bool TitleIsValid() => Title.Trim().Length > 0;
}
