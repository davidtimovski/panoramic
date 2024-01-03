using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain;

namespace Panoramic.ViewModels.Widgets;

public partial class SettingsViewModel(string defaultTitle, IWidgetData data) : ObservableObject
{
    [ObservableProperty]
    private Area area = data.Area;

    [ObservableProperty]
    private string title = data.Id == Guid.Empty ? defaultTitle : data.Title;

    protected bool TitleIsValid() => Title.Trim().Length > 0;
}
