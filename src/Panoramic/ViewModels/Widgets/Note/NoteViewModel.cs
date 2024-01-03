using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Panoramic.Models.Domain.Note;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteViewModel : ObservableObject
{
    public NoteViewModel(NoteWidget widget)
    {
        Title = widget.Title;
        Text = widget.Text;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    private bool highlighted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditorVisibility))]
    [NotifyPropertyChangedFor(nameof(PresenterVisibility))]
    [NotifyPropertyChangedFor(nameof(EditToggleTooltip))]
    private bool editing;

    [ObservableProperty]
    private string text;

    public Visibility EditorVisibility => Editing ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PresenterVisibility => Editing ? Visibility.Collapsed : Visibility.Visible;

    public string EditToggleTooltip => Editing ? "Stop editing" : "Edit";

    public SolidColorBrush Background => Highlighted
        ? (Application.Current.Resources["PanoramicWidgetHighlightedBackgroundBrush"] as SolidColorBrush)!
        : (Application.Current.Resources["PanoramicWidgetBackgroundBrush"] as SolidColorBrush)!;
}
