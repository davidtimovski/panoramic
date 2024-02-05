using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;

namespace Panoramic.ViewModels.Widgets;

public partial class SettingsViewModel(IWidgetData data) : ObservableObject
{
    [ObservableProperty]
    private Area area = data.Area;
}
