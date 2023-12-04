using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models;

namespace Panoramic.UserControls;

public sealed partial class WidgetPicker : UserControl
{
    public WidgetPicker(string section)
    {
        InitializeComponent();

        Section = $"Section: {section}";

        WidgetClicked = new RelayCommand<string>((widgetType) =>
        {
            var type = Enum.Parse<WidgetType>(widgetType!);
            WidgetPicked?.Invoke(this, new WidgetPickedEventArgs(section, type));
        });
    }

    public string Section { get; }

    public event EventHandler<WidgetPickedEventArgs>? WidgetPicked;
    public RelayCommand<string> WidgetClicked { get; }
}

public class WidgetPickedEventArgs : EventArgs
{
    public WidgetPickedEventArgs(string section, WidgetType type)
    {
        Section = section;
        Type = type;
    }

    public string Section { get; }
    public WidgetType Type { get; }
}
