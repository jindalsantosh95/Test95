#nullable enable
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Helpers.FaultInjection;

/// <summary>Optional sequential proxy lifecycle. Does not change system proxy settings or certificate trust.</summary>
public sealed class FaultProxySession : IAsyncDisposable
{
    private readonly FileStream _lease;
    private readonly Process _process;
    private readonly string _directory;
    private readonly string _certificateDirectory;
    private bool _disposed;
    private long _expiresUnixSeconds;

    private FaultProxySession(FileStream lease, Process process, string directory, int port, string? certificateDirectory)
    {
        _lease = lease;
        _process = process;
        _directory = directory;
        _certificateDirectory = certificateDirectory ?? Path.Combine(directory, "ca");
        ProxyAddress = new Uri($"http://127.0.0.1:{port}");
    }

    public Uri ProxyAddress { get; }
    public string CertificatePath => Path.Combine(_certificateDirectory, "mitmproxy-ca-cert.pem");
    public bool WasInjected => File.Exists(Path.Combine(_directory, "injected.json"));

    public static async Task<FaultProxySession> StartAsync(string executable, string addonPath,
        JObject rule, int port = 8899, string? certificateDirectory = null, CancellationToken cancellationToken = default,
        int? localCaptureProcessId = null)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (!Path.IsPathFullyQualified(executable) || !File.Exists(executable))
            throw new ArgumentException("An installed proxy executable's absolute path is required.", nameof(executable));
        if (!Path.IsPathFullyQualified(addonPath) || !File.Exists(addonPath))
            throw new ArgumentException("An absolute reviewed addon path is required.", nameof(addonPath));
        if (port is < 1024 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));
        if (localCaptureProcessId is <= 0)
            throw new ArgumentOutOfRangeException(nameof(localCaptureProcessId));
        if (localCaptureProcessId.HasValue && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Process-scoped capture is available only for the macOS POC.");
        if (certificateDirectory is not null && (!Path.IsPathFullyQualified(certificateDirectory)
            || !File.Exists(Path.Combine(certificateDirectory, "mitmproxy-ca.pem"))))
            throw new ArgumentException("A provisioned isolated CA directory is required.", nameof(certificateDirectory));
        // The adapter independently validates its allowlist. The host never grants arbitrary upstream scope.
        Uri endpoint = new Uri(rule.Value<string>("endpoint") ?? throw new ArgumentException("Missing endpoint."));
        if (endpoint.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(endpoint.UserInfo))
            throw new ArgumentException("An HTTPS endpoint without credentials is required.");

        FileStream lease = new FileStream(Path.Combine(Path.GetTempPath(), "eapi-fault-proxy.lock"),
            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string directory = Path.Combine(Path.GetTempPath(), "eapi-fault-" + Guid.NewGuid().ToString("N"));
        Process process = new Process();
        FaultProxySession session = new FaultProxySession(lease, process, directory, port, certificateDirectory);
        try
        {
            Directory.CreateDirectory(directory);
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            JObject boundedRule = (JObject)rule.DeepClone();
            session._expiresUnixSeconds = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            boundedRule["expiresUnixSeconds"] = session._expiresUnixSeconds;
            await File.WriteAllTextAsync(Path.Combine(directory, "rule.json"), boundedRule.ToString(Formatting.None), cancellationToken);
            ProcessStartInfo start = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (string argument in new[] { "--listen-host", "127.0.0.1", "--listen-port", port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "--quiet", "--set", "flow_detail=0", "--set", "connection_strategy=lazy", "--set", "upstream_cert=false",
                "--set", "confdir=" + session._certificateDirectory,
                "--set", "allow_hosts=^" + System.Text.RegularExpressions.Regex.Escape(endpoint.Host) + "(:443)?$", "-s", addonPath })
                start.ArgumentList.Add(argument);
            if (localCaptureProcessId.HasValue)
            {
                start.ArgumentList.Add("--mode");
                start.ArgumentList.Add("local:" + localCaptureProcessId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            start.Environment["EAPI_FAULT_DIRECTORY"] = directory;
            start.Environment["PYTHONPATH"] = Path.GetDirectoryName(addonPath);
            process.StartInfo = start;
            // Drain without recording potentially sensitive proxy diagnostics.
            process.OutputDataReceived += (_, _) => { };
            process.ErrorDataReceived += (_, _) => { };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await session.WaitForEventAsync("ready", TimeSpan.FromSeconds(30), cancellationToken);
            return session;
        }
        catch
        {
            await session.DisposeAsync();
            throw;
        }
    }

    public Task WaitForInjectionAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        => WaitForEventAsync("injected", timeout, cancellationToken);

    public Task WaitForWithheldRequestAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        => WaitForEventAsync("withheld", timeout, cancellationToken);

    public void EnsureNoGuardViolation()
    {
        if (_disposed || _process.HasExited || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= _expiresUnixSeconds)
            throw new InvalidOperationException("Fault proxy is no longer active within its bounded lifetime. Do not submit or retry payment.");
        if (File.Exists(Path.Combine(_directory, "guard-blocked.json")))
            throw new InvalidOperationException("Completion guard blocked an unexpected or expired request. No fault-coverage pass can be claimed.");
    }

    public string ReadEventSummary()
    {
        List<string> events = new();
        foreach (string name in new[] { "ready", "injected", "withheld", "hold-completed", "guard-blocked" })
        {
            string path = Path.Combine(_directory, name + ".json");
            if (!File.Exists(path))
                continue;
            try
            {
                JObject metadata = JObject.Parse(File.ReadAllText(path));
                string timestamp = DateTimeOffset.TryParse(metadata.Value<string>("utc"), out DateTimeOffset utc)
                    ? utc.ToUniversalTime().ToString("O") : "timestamp unavailable";
                events.Add($"{name}={timestamp}");
            }
            catch (Exception exception) when (exception is IOException or JsonException)
            {
                events.Add($"{name}=metadata unavailable");
            }
        }
        return string.Join("; ", events);
    }

    private async Task WaitForEventAsync(string name, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(10))
            throw new ArgumentOutOfRangeException(nameof(timeout));
        using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);
        try
        {
            while (true)
            {
                if (_process.HasExited)
                    throw new InvalidOperationException($"Fault proxy stopped before {name}; exit code {_process.ExitCode}. No raw traffic logs retained.");
                if (File.Exists(Path.Combine(_directory, name + ".json")))
                    return;
                await Task.Delay(100, deadline.Token);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Fault proxy did not report {name} within {timeout.TotalSeconds} seconds.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await _process.WaitForExitAsync(deadline.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // The exit wait has its own deadline, not the test's cancellation token.
                // Preserve test evidence and continue directory/lease cleanup on expiry.
                Console.WriteLine("Fault proxy cleanup: process exit was not confirmed within 10 seconds; continuing private-directory and lease cleanup.");
            }
            catch (InvalidOperationException) { /* Process never started or has already exited. */ }
            finally { _process.Dispose(); }
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
        finally { _lease.Dispose(); }
    }
}
