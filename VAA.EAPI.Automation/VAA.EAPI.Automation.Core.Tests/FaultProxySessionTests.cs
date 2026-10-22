using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers.FaultInjection;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class FaultProxySessionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Start_InvalidCapturePid_RejectsBeforeStartingProcess(int processId)
    {
        string existing = typeof(FaultProxySessionTests).Assembly.Location;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => FaultProxySession.StartAsync(
            existing, existing, new JObject(), localCaptureProcessId: processId));
    }

    [Fact]
    public async Task Start_ProcessExitsBeforeReadiness_ReleasesLeaseForNextAttempt()
    {
        string executable = Path.GetFullPath(Path.Combine(
            System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "../../..",
            OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"));
        Assert.True(File.Exists(executable), "The test requires the installed dotnet host.");
        JObject rule = new JObject { ["endpoint"] = "https://example.invalid" };
        // dotnet rejects mitmdump arguments without opening a listener or sending network traffic.
        for (int attempt = 0; attempt < 2; attempt++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => FaultProxySession.StartAsync(
                executable, typeof(FaultProxySessionTests).Assembly.Location, rule));
        }
    }

    [Fact]
    public async Task Start_MissingExecutable_RejectsBeforeStartingProcess()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => FaultProxySession.StartAsync(
            "not-an-absolute-executable", "addon.py", new JObject()));
    }

    [Fact]
    public async Task Start_MissingAddon_RejectsBeforeStartingProcess()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => FaultProxySession.StartAsync(
            typeof(FaultProxySessionTests).Assembly.Location, "missing.py", new JObject()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1023)]
    [InlineData(65536)]
    public async Task Start_InvalidPort_RejectsBeforeStartingProcess(int port)
    {
        string existing = typeof(FaultProxySessionTests).Assembly.Location;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => FaultProxySession.StartAsync(
            existing, existing, new JObject(), port));
    }

    [Theory]
    [InlineData("http://example.invalid")]
    [InlineData("https://user:password@example.invalid")]
    public async Task Start_InsecureEndpoint_RejectsBeforeStartingProcess(string endpoint)
    {
        string existing = typeof(FaultProxySessionTests).Assembly.Location;
        await Assert.ThrowsAsync<ArgumentException>(() => FaultProxySession.StartAsync(
            existing, existing, new JObject { ["endpoint"] = endpoint }));
    }
}
