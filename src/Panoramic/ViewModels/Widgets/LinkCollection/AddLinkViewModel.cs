using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels;

public partial class AddLinkViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string uri = string.Empty;

    public event EventHandler<ValidationEventArgs>? Validated;

    public void ValidateAndEmit()
    {
        if (Title.Trim().Length == 0)
        {
            Validated?.Invoke(this, new ValidationEventArgs(false));
            return;
        }

        var uri = Uri.Trim();
        if (uri.Length > 0)
        {
            if (!System.Uri.TryCreate(uri, UriKind.Absolute, out var _))
            {
                Validated?.Invoke(this, new ValidationEventArgs(false));
                return;
            }
        }
        else
        {
            Validated?.Invoke(this, new ValidationEventArgs(false));
            return;
        }

        Validated?.Invoke(this, new ValidationEventArgs(true));
    }
}
