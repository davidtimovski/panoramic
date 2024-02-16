using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class FolderRenameViewModel(string path) : ObservableObject
{
    private readonly string _directory = Path.GetDirectoryName(path)!;

    [ObservableProperty]
    private string name = Path.GetFileName(path)!;

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(CanBeCreated()));

    private bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.FolderCanBeCreated(Name.Trim(), _directory);
}
