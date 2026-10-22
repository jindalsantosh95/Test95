using System.Net;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class IosAppLogEvidenceTests
{
    private const string Correlation = "11111111-2222-3333-4444-555555555555";

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AppLogs_OnlyRetainSafeMetadataNotMessagesOrCredentials()
    {
        string json = Logs($"vaa-mobile-ios[123:abc] failed statusCode=502 api-correlation-id={Correlation} " +
            "password=secret token=abc passenger=Jane card=4111111111111111 email=test@example.com INTERNAL_SERVER_ERROR");
        string summary = Assert.Single(IosAppLogEvidence.Sanitise(json, "vaa-mobile-ios", DateTimeOffset.UtcNow.AddMinutes(-1)));
        Assert.Contains("httpStatus=502", summary);
        Assert.Contains(Correlation, summary);
        Assert.Contains("recognisedCode=INTERNAL_SERVER_ERROR", summary);
        foreach (string secret in new[] { "secret", "Jane", "4111111111111111", "test@example.com", "token=abc" })
        {
            Assert.DoesNotContain(secret, summary);
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("OtherApp[123] failed status=500")]
    [InlineData("OtherApp[123] mentions vaa-mobile-ios[123] failed status=500")]
    [InlineData("not-vaa-mobile-ios[123] failed status=500")]
    [InlineData("vaa-mobile-ios[123] normal navigation")]
    public void AppLogs_ExcludeOtherProcessesAndUninterestingLines(string message)
        => Assert.Empty(IosAppLogEvidence.Sanitise(Logs(message), "vaa-mobile-ios", DateTimeOffset.UtcNow.AddMinutes(-1)));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AppLogs_UnknownErrorsAreNotCopiedIntoReport()
    {
        string summary = Assert.Single(IosAppLogEvidence.Sanitise(
            Logs("vaa-mobile-ios[123] errorCode=PASSWORD_SECRET response={sensitive}"), "vaa-mobile-ios", DateTimeOffset.UtcNow.AddMinutes(-1)));
        Assert.Contains("signal=app-error; detail=omitted", summary);
        Assert.DoesNotContain("PASSWORD_SECRET", summary);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AppLogs_ExcludeOldAndOversizedEntriesAndBoundOutput()
    {
        JArray entries = new();
        for (int index = 0; index < 100; index++)
        {
            entries.Add(new JObject
            {
                ["timestamp"] = DateTimeOffset.UtcNow.AddSeconds(-index - 1).ToUnixTimeMilliseconds(),
                ["message"] = "vaa-mobile-ios[123] failed status=500",
            });
        }
        entries.Add(new JObject { ["timestamp"] = 0, ["message"] = "vaa-mobile-ios[123] failed status=503" });
        entries.Add(new JObject { ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ["message"] = "vaa-mobile-ios[123] failed " + new string('x', 17000) });
        IReadOnlyList<string> summaries = IosAppLogEvidence.Sanitise(new JObject { ["value"] = entries }.ToString(), "vaa-mobile-ios", DateTimeOffset.UtcNow.AddMinutes(-10));
        Assert.Equal(40, summaries.Count);
        Assert.All(summaries, summary => Assert.Contains("httpStatus=500", summary));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("https://example.com")]
    [InlineData("http://user:password@localhost:4723")]
    [InlineData("http://localhost:4723?token=secret")]
    public void AppLogs_RejectNonLocalOrCredentialBearingEndpoints(string server)
        => Assert.Throws<InvalidOperationException>(() => IosAppLogEvidence.Endpoint(server, Correlation));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public async Task AppLogs_UseExistingBufferEndpointAndSanitiseInMemory()
    {
        using HttpClient client = new(new LogHandler());
        IReadOnlyList<string> summaries = await IosAppLogEvidence.CollectAsync(
            "http://localhost:4723/wd/hub", Correlation, "vaa-mobile-ios", DateTimeOffset.UtcNow.AddMinutes(-1), client);
        Assert.Contains("httpStatus=503", Assert.Single(summaries));
    }

    private static string Logs(string message) => new JObject
    {
        ["value"] = new JArray(new JObject
        {
            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ["message"] = message,
        }),
    }.ToString();

    // Fake the local driver transport, not an EAPI/backend service.
    private sealed class LogHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.EndsWith($"/wd/hub/session/{Correlation}/se/log", request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Logs("vaa-mobile-ios[123] failed status=503 password=secret")),
            });
        }
    }
}
