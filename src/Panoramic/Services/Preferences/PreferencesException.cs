using System;

namespace Panoramic.Services.Preferences;

public class PreferencesException : Exception
{
    public PreferencesException(Exception innerException) : this("An unexpected error occurred", innerException) { }
    public PreferencesException(string message, Exception innerException) : base(message, innerException) { }
}
