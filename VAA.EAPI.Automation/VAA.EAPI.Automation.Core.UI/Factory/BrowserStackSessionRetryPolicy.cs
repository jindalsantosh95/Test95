namespace VAA.EAPI.Automation.Core.UI.Factory;

public static class BrowserStackSessionRetryPolicy
{
    private static readonly string[] TransientMarkers =
    {
        "BROWSERSTACK_SO_TIMEOUT",
        "BROWSERSTACK_COULD_NOT_LAUNCH_APP",
        "BROWSERSTACK_DEVICE_NOT_AVAILABLE",
        "BROWSERSTACK_DEVICE_SETUP_FAILED",
        "502 Bad Gateway",
        "503 Service Unavailable",
        "504 Gateway Timeout",
        "connection was closed",
        "connection reset",
        "timed out",
    };

    public static bool IsTransient(Exception exception)
    {
        string message = exception.ToString();
        return TransientMarkers.Any(marker =>
            message.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
