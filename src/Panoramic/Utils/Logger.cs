using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Panoramic.Utils;

public static class Logger
{
    private static readonly ILogger DebugLogger;

    static Logger()
    {
#if DEBUG
        using var provider = new DebugLoggerProvider();
        DebugLogger = provider.CreateLogger("Debug");
#endif
    }

    public static void LogDebug(string message)
    {
#if DEBUG
        DebugLogger.LogDebug("{Message}", message);
#endif
    }
}
