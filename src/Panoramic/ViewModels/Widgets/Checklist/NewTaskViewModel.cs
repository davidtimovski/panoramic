using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed partial class NewTaskViewModel(ChecklistWidget widget) : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    partial void OnTitleChanged(string value) => ValidateAndEmit();

    [ObservableProperty]
    public partial DateTimeOffset? DueDate { get; set; }

    [ObservableProperty]
    public partial string Url { get; set; } = string.Empty;

    partial void OnUrlChanged(string value) => ValidateAndEmit();

    public event EventHandler<ValidationEventArgs>? Validated;

    public bool CanBeCreated() => Title.Trim().Length > 0
        && widget.TaskCanBeCreated(Title.Trim())
        && (Url.Trim().Length == 0 || Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var _));

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
}
