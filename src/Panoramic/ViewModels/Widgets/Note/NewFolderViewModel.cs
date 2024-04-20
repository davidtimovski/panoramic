using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed class NewFolderViewModel(string directory) : ObservableObject
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
                Validated?.Invoke(this, new ValidationEventArgs { Valid = CanBeCreated() });
            }
        }
    }

    public event EventHandler<ValidationEventArgs>? Validated;

    public bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.FolderCanBeCreated(Name.Trim(), directory);
}
