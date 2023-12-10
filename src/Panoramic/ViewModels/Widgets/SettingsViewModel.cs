using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Services.Storage.Models;

namespace Panoramic.ViewModels.Widgets;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(string defaultTitle, WidgetData data)
    {
        area = data.Area;
        title = data.Id == Guid.Empty ? defaultTitle : data.Title;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreaLabel))]
    private Area area;

    public string AreaLabel => $"Area: {Area}";

    [ObservableProperty]
    private string title;

    protected bool TitleIsValid() => Title.Trim().Length > 0;
}
