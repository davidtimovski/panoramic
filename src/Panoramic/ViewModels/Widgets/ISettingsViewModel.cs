using System;
using Panoramic.Models.Domain;
using Panoramic.Models.Events;

namespace Panoramic.ViewModels.Widgets;

public interface ISettingsViewModel
{
    Area Area { get; set; }

    /// <summary>
    /// Used for attaching the validation handler later, after ViewModel creation.
    /// </summary>
    void AttachValidationHandler(EventHandler<ValidationEventArgs> handler);
}
