using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.UI.Helpers;

public static class BrowserStackSessionCleanup
{
    private static readonly TimeSpan CleanupTimeout = TimeSpan.FromSeconds(15);

    public static async Task<bool> TryTerminateAsync(
        IConfiguration config,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId.IsNullOrWhiteSpace())
            return false;

        string user = config["BrowserStack:Username"] ?? string.Empty;
        string key = config["BrowserStack:AccessKey"] ?? string.Empty;
        if (user.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
            return false;

        string serverUrl = config["BrowserStack:ServerUrl"]
            ?? throw new InvalidOperationException("Missing BrowserStack:ServerUrl");
        Uri endpoint = new($"{serverUrl.TrimEnd('/')}/session/{Uri.EscapeDataString(sessionId)}");

        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CleanupTimeout);
        using HttpClient client = new();
        using HttpRequestMessage request = new(HttpMethod.Delete, endpoint);
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{key}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        try
        {
            using HttpResponseMessage response = await client.SendAsync(request, timeout.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception exception) when (
            exception is HttpRequestException ||
            exception is TaskCanceledException ||
            exception is OperationCanceledException)
        {
            Console.WriteLine(
                $"BrowserStack fallback cleanup failed for session {sessionId}: {exception.Message}");
            return false;
        }
    }
}
