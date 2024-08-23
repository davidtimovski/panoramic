using System;

namespace Panoramic.Services.Drawers;

public class DrawerException : Exception
{
    public DrawerException(Exception innerException) : this("An unexpected error occurred", innerException) { }
    public DrawerException(string message, Exception innerException) : base(message, innerException) { }
}
