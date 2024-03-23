using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed partial class NoteRenameViewModel(string path) : ObservableObject
{
    private readonly string _directory = Path.GetDirectoryName(path)!;

    private string name = Path.GetFileNameWithoutExtension(path);
    public string Name
    {
        get => name;
        set
        {
            if (SetProperty(ref name, value))
            {
                OnPropertyChanged(nameof(Name));
                ValidateAndEmit();
            }
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });

    private bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.NoteCanBeCreated(Name, _directory);
}
