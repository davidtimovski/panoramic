using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Panoramic.Utils;

public static class DebugLogger
{
    private static readonly ILogger Logger;

    static DebugLogger()
    {
#if DEBUG
        using var provider = new DebugLoggerProvider();
        Logger = provider.CreateLogger("Debug");
#endif
    }

    public static void Log(string message)
    {
#if DEBUG
        Logger.LogDebug("{Message}", message);
#endif
    }
}
