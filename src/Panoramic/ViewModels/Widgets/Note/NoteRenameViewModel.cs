﻿using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Domain.Note;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets.Note;

public partial class NoteRenameViewModel(string path) : ObservableObject
{
    private readonly string _directory = Path.GetDirectoryName(path)!;

    [ObservableProperty]
    private string name = Path.GetFileNameWithoutExtension(path);

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit() => Validated?.Invoke(this, new ValidationEventArgs(CanBeCreated()));

    private bool CanBeCreated() => Name.Trim().Length > 0 && NoteWidget.NoteCanBeCreated(Name.Trim(), _directory);
}
