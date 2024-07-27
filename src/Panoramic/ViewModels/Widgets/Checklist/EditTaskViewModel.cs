using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class EditTaskViewModel : ObservableObject
{
    private readonly SolidColorBrush _fieldForegroundBrush;
    private readonly SolidColorBrush _fieldChangedForegroundBrush;

    public EditTaskViewModel(
        string title,
        DateTimeOffset? dueDate,
        Uri? uri,
        DateTime created,
        SolidColorBrush fieldForegroundBrush,
        SolidColorBrush fieldChangedForegroundBrush)
        : this(title, dueDate, uri, created, false, fieldForegroundBrush, fieldChangedForegroundBrush) { }

    public EditTaskViewModel(
        string title,
        DateTimeOffset? dueDate,
        Uri? uri,
        DateTime created,
        bool changed,
        SolidColorBrush fieldForegroundBrush,
        SolidColorBrush fieldChangedForegroundBrush)
    {
        _fieldForegroundBrush = fieldForegroundBrush;
        _fieldChangedForegroundBrush = fieldChangedForegroundBrush;

        _originalTitle = changed ? string.Empty : title;
        Title = title;

        _originalDueDate = changed ? DateTimeOffset.MinValue : dueDate;
        DueDate = dueDate;

        _originalUrl = changed || uri is null ? string.Empty : uri.ToString();
        Url = uri is not null ? uri.ToString() : string.Empty;
        Created = created;
    }

    public event EventHandler<EventArgs>? Updated;

    private readonly string _originalTitle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForegroundBrush))]
    private string title = string.Empty;

    partial void OnTitleChanged(string value) => Updated?.Invoke(this, EventArgs.Empty);

    public SolidColorBrush TitleForegroundBrush => Title.Equals(_originalTitle, StringComparison.Ordinal) && DueDateHasNotChanged()
        ? _fieldForegroundBrush
        : _fieldChangedForegroundBrush;

    private readonly DateTimeOffset? _originalDueDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForegroundBrush))]
    [NotifyPropertyChangedFor(nameof(UrlForegroundBrush))]
    private DateTimeOffset? dueDate;

    private readonly string _originalUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UrlForegroundBrush))]
    private string url = string.Empty;

    partial void OnUrlChanged(string value)
    {
        Uri = Url.Trim().Length > 0 && Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var createdUri) ? createdUri : null;
        NavigationIsEnabled = Uri is not null;

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public SolidColorBrush UrlForegroundBrush => Url.Equals(_originalUrl, StringComparison.Ordinal) && DueDateHasNotChanged()
        ? _fieldForegroundBrush
        : _fieldChangedForegroundBrush;

    public DateTime Created { get; }

    [ObservableProperty]
    private Uri? uri;

    [ObservableProperty]
    private bool navigationIsEnabled;

    public bool IsValid() => Title.Trim().Length > 0
        && (Url.Trim().Length == 0 || Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _));

    private bool DueDateHasNotChanged()
    {
        if (DueDate.HasValue && _originalDueDate.HasValue && DueDate.Value.Date.Equals(_originalDueDate.Value.Date))
        {
            return true;
        }

        if (!DueDate.HasValue && !_originalDueDate.HasValue)
        {
            return true;
        }

        return false;
    }
}
