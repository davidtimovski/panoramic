using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Panoramic.Utils;

#if DEBUG
public static class DebugLogger
{
    private static readonly ILogger Logger;

    static DebugLogger()
    {
        using var provider = new DebugLoggerProvider();
        Logger = provider.CreateLogger("Debug");
    }

    public static void Log(string message)
    {
        Logger.LogDebug("{Message}", message);
    }
}
#else
public static class DebugLogger
{
    public static void Log(string _)
    {
    }
}
#endif