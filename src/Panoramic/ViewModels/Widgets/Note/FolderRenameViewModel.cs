﻿using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public sealed class FolderRenameViewModel(string path) : ObservableObject
{
    private readonly string _directory = Path.GetDirectoryName(path)!;

    private string name = Path.GetFileName(path);
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

    public bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.FolderCanBeCreated(Name.Trim(), _directory);
}
