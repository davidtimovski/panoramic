using System;
using Panoramic.Data;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets;

public interface ISettingsViewModel
{
    Area Area { set; }

    /// <summary>
    /// Used for attaching the validation handler later, after ViewModel creation.
    /// </summary>
    void AttachValidationHandler(EventHandler<ValidationEventArgs> handler);
}
