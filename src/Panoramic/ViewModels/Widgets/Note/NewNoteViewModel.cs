using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed class NewNoteViewModel(string directory) : ObservableObject
{
    private string name = string.Empty;
    public string Name
    {
        get => name;
        set
        {
            if (SetProperty(ref name, value))
            {
                OnPropertyChanged();
                ValidateAndEmit();
            }
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    private void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });

    private bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.NoteCanBeCreated(Name.Trim(), directory);
}
