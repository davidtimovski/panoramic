using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NewNoteViewModel(string directory) : ObservableObject
{
    private readonly string _directory = directory;

    [ObservableProperty]
    private string name = string.Empty;

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(CanBeCreated()));

    private bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.NoteCanBeCreated(Name.Trim(), _directory);
}
