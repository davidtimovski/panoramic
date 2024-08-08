using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class FolderRenameViewModel(string absolutePath) : ObservableObject
{
    private readonly string _directory = Path.GetDirectoryName(absolutePath)!;

    [ObservableProperty]
    private string name = Path.GetFileName(absolutePath);
    partial void OnNameChanged(string value) => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });

    public event EventHandler<ValidationEventArgs>? Validated;

    public bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.FolderCanBeCreated(Name.Trim(), _directory);
}
