using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Panoramic.Models;

namespace Panoramic.UserControls;

public sealed partial class WidgetPicker : UserControl
{
    private readonly Dictionary<WidgetType, ToggleButton> _toggleLookup = [];

    public WidgetPicker(WidgetType? widgetType)
    {
        InitializeComponent();

        WidgetClicked = new RelayCommand<string>(WidgetToggled);

        _toggleLookup.Add(WidgetType.Note, NoteToggle);
        _toggleLookup.Add(WidgetType.LinkCollection, LinkCollectionToggle);
        _toggleLookup.Add(WidgetType.RecentLinks, RecentLinksToggle);

        if (widgetType is not null)
        {
            _toggleLookup[widgetType.Value].IsChecked = true;
        }
    }

    public event EventHandler<WidgetPickedEventArgs>? WidgetPicked;
    public event EventHandler<EventArgs>? WidgetDeselected;

    public RelayCommand<string> WidgetClicked { get; }

    private void WidgetToggled(string? widgetType)
    {
        var type = Enum.Parse<WidgetType>(widgetType!);

        var clickedToggle = _toggleLookup[type];

        if (clickedToggle.IsChecked == true)
        {
            var otherToggles = _toggleLookup.Where(x => x.Key != type).Select(x => x.Value).ToList();
            foreach (var toggle in otherToggles)
            {
                toggle.IsChecked = false;
            }

            WidgetPicked?.Invoke(this, new WidgetPickedEventArgs(type));
        }
        else
        {
            WidgetDeselected?.Invoke(this, new EventArgs());
        }
    }
}

public sealed class WidgetPickedEventArgs(WidgetType type) : EventArgs
{
    public WidgetType Type { get; } = type;
}
