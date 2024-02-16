using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Panoramic.Models.Domain;
using Panoramic.Services.Storage;

namespace Panoramic.UserControls;

public sealed partial class AreaPicker : UserControl
{
    private readonly IStorageService _storageService;
    private readonly Guid? _widgetId;

    private Dictionary<string, ToggleButton>? availableTogglesLookup;

    public AreaPicker(IStorageService storageService, Guid? widgetId)
    {
        InitializeComponent();

        _storageService = storageService;
        _widgetId = widgetId;

        Initialize();

        SectionToggled = new RelayCommand<string>(CreateSelectionArea);
    }

    /// <summary>
    /// Preselect sections when editing.
    /// Hide sections that are used by other widgets.
    /// </summary>
    private void Initialize()
    {
        availableTogglesLookup = Grid.Children.OfType<ToggleButton>().ToDictionary(x => x.Name.Substring(6, 2), x => x);

        if (_widgetId is not null)
        {
            var widget = _storageService.Widgets[_widgetId.Value];

            int xStart = widget.Area.Column;
            int yStart = widget.Area.Row;
            int xEnd = widget.Area.Column + widget.Area.ColumnSpan - 1;
            int yEnd = widget.Area.Row + widget.Area.RowSpan - 1;

            for (var i = xStart; i <= xEnd; i++)
            {
                for (var j = yStart; j <= yEnd; j++)
                {
                    var key = $"{j}{i}";
                    availableTogglesLookup[key].IsChecked = true;
                }
            }
        }

        // Hide used sections
        var otherWidgets = _widgetId is not null
            ? _storageService.Widgets.Where(x => x.Key != _widgetId)
            : _storageService.Widgets;
        foreach (var widget in otherWidgets)
        {
            int xStart = widget.Value.Area.Column;
            int yStart = widget.Value.Area.Row;
            int xEnd = widget.Value.Area.Column + widget.Value.Area.ColumnSpan - 1;
            int yEnd = widget.Value.Area.Row + widget.Value.Area.RowSpan - 1;

            for (var i = xStart; i <= xEnd; i++)
            {
                for (var j = yStart; j <= yEnd; j++)
                {
                    var key = $"{j}{i}";

                    // Hide
                    availableTogglesLookup[key].Visibility = Visibility.Collapsed;

                    // Remove from toggles
                    availableTogglesLookup.Remove(key);
                }
            }
        }
    }

    private void CreateSelectionArea(string? section)
    {
        var checkedToggles = availableTogglesLookup!.Where(x => x.Value.IsChecked == true).ToList();
        if (checkedToggles.Count == 1)
        {
            // Skip if there's only one section toggled
            AreaReset?.Invoke(this, new EventArgs());
            return;
        }

        if (checkedToggles.Count > 2)
        {
            // If there's already a selected area, reset it and only leave the currently selected toggle
            var checkedTogglesExceptCurrent = checkedToggles.Where(x => !string.Equals(x.Key, section, StringComparison.Ordinal)).ToList();
            foreach (var toggleKvp in checkedTogglesExceptCurrent)
            {
                toggleKvp.Value.IsChecked = false;
            }
            AreaReset?.Invoke(this, new EventArgs());
            return;
        }

        var x1 = int.Parse(checkedToggles[0].Key[1].ToString(), CultureInfo.InvariantCulture);
        var y1 = int.Parse(checkedToggles[0].Key[0].ToString(), CultureInfo.InvariantCulture);
        var x2 = int.Parse(checkedToggles[1].Key[1].ToString(), CultureInfo.InvariantCulture);
        var y2 = int.Parse(checkedToggles[1].Key[0].ToString(), CultureInfo.InvariantCulture);

        int xStart;
        int yStart;
        int xEnd;
        int yEnd;
        if (x1 < x2)
        {
            xStart = x1;
            xEnd = x2;
        }
        else
        {
            xStart = x2;
            xEnd = x1;
        }

        if (y1 < y2)
        {
            yStart = y1;
            yEnd = y2;
        }
        else
        {
            yStart = y2;
            yEnd = y1;
        }

        var togglesToCheck = new List<ToggleButton>();
        var overlappingWithUsedAreas = false;
        for (var i = xStart; i <= xEnd; i++)
        {
            // Find first selected
            for (var j = yStart; j <= yEnd; j++)
            {
                var key = $"{j}{i}";

                if (availableTogglesLookup!.TryGetValue(key, out var toggle))
                {
                    togglesToCheck.Add(toggle);
                }
                else
                {
                    // Skip used section
                    overlappingWithUsedAreas = true;
                    break;
                }
            }
        }

        if (overlappingWithUsedAreas)
        {
            // Overlap detected, uncheck toggles
            foreach (var toggle in checkedToggles)
            {
                toggle.Value.IsChecked = false;
            }
            AreaReset?.Invoke(this, new EventArgs());
            return;
        }

        // Check all toggles between the two
        foreach (var toggle in togglesToCheck)
        {
            toggle.IsChecked = true;
        }

        var selectedArea = new Area($"{yStart}{xStart}-{yEnd}{xEnd}");
        AreaPicked?.Invoke(this, new AreaPickedEventArgs(selectedArea));
    }

    public RelayCommand<string> SectionToggled { get; }

    public event EventHandler<AreaPickedEventArgs>? AreaPicked;

    public event EventHandler<EventArgs>? AreaReset;
}

public sealed class AreaPickedEventArgs(Area area) : EventArgs
{
    public Area Area { get; } = area;
}
