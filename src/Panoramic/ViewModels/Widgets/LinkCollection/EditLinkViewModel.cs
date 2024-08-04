﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Panoramic.ViewModels.Widgets.LinkCollection;

public sealed partial class EditLinkViewModel : ObservableObject
{
    private readonly SolidColorBrush _fieldForegroundBrush;
    private readonly SolidColorBrush _fieldChangedForegroundBrush;

    public EditLinkViewModel(string title, Uri uri, SolidColorBrush fieldForegroundBrush, SolidColorBrush fieldChangedForegroundBrush)
        : this(title, uri, false, fieldForegroundBrush, fieldChangedForegroundBrush) { }

    public EditLinkViewModel(string title, Uri uri, bool changed, SolidColorBrush fieldForegroundBrush, SolidColorBrush fieldChangedForegroundBrush)
    {
        _fieldForegroundBrush = fieldForegroundBrush;
        _fieldChangedForegroundBrush = fieldChangedForegroundBrush;

        _originalTitle = changed ? string.Empty : title;
        Title = title;

        _originalUrl = changed ? string.Empty : uri.ToString();
        Url = uri.ToString();
    }

    public event EventHandler<EventArgs>? Updated;

    private readonly string _originalTitle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleForegroundBrush))]
    private string title = string.Empty;
    partial void OnTitleChanged(string value) => Updated?.Invoke(this, EventArgs.Empty);

    public SolidColorBrush TitleForegroundBrush => Title.Trim().Equals(_originalTitle, StringComparison.Ordinal) ? _fieldForegroundBrush : _fieldChangedForegroundBrush;

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

    public SolidColorBrush UrlForegroundBrush => Url.Trim().Equals(_originalUrl, StringComparison.Ordinal) ? _fieldForegroundBrush : _fieldChangedForegroundBrush;

    [ObservableProperty]
    private Uri? uri;

    [ObservableProperty]
    private bool navigationIsEnabled;

    public bool IsValid() => Title.Trim().Length > 0 && Url.Trim().Length > 0 && Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _);
}
