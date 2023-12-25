using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;

namespace Panoramic.ViewModels.Widgets;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(string defaultTitle, WidgetData data)
    {
        area = data.Area;
        title = data.Id == Guid.Empty ? defaultTitle : data.Title;
    }

    [ObservableProperty]
    private Area area;

    [ObservableProperty]
    private string title;

    protected bool TitleIsValid() => Title.Trim().Length > 0;
}
