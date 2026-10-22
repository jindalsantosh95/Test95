using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class BrowserStackSessionStatusClientTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(200, true)]
    [InlineData(401, false)]
    [InlineData(302, false)]
    [InlineData(500, false)]
    public async Task StatusUpdate_IsSingleMetadataRequest(int code, bool expected)
    {
        using RecordingHandler handler = new((HttpStatusCode)code);
        using HttpClient client = new(handler);
        BrowserStackSessionStatusClient reporter = new(client, Configuration());
        Assert.Equal(expected, await reporter.UpdateAsync(new string('a', 40), "failed", "quote\" newline\n tab\t"));
        Assert.Equal(1, handler.Calls);
        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal("https://api-cloud.browserstack.com/app-automate/sessions/" + new string('a', 40) + ".json", handler.Url);
        using JsonDocument body = JsonDocument.Parse(handler.Body);
        Assert.Equal("failed", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("quote\" newline\n tab\t", body.RootElement.GetProperty("reason").GetString());
        Assert.True(handler.HasAuthorization);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("../other", "failed", true)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "unknown", true)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "failed", false)]
    public async Task InvalidConfiguration_DoesNotSend(string session, string status, bool credentials)
    {
        using RecordingHandler handler = new(HttpStatusCode.OK);
        using HttpClient client = new(handler);
        Assert.False(await new BrowserStackSessionStatusClient(client, Configuration(credentials))
            .UpdateAsync(session, status, "synthetic reason"));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public async Task LongReason_IsBounded()
    {
        using RecordingHandler handler = new(HttpStatusCode.OK);
        using HttpClient client = new(handler);
        Assert.True(await new BrowserStackSessionStatusClient(client, Configuration())
            .UpdateAsync(new string('a', 40), "passed", new string('x', 300)));
        using JsonDocument body = JsonDocument.Parse(handler.Body);
        Assert.Equal(255, body.RootElement.GetProperty("reason").GetString()!.Length);
    }

    private static IConfiguration Configuration(bool credentials = true)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BrowserStack:Username"] = credentials ? "synthetic-user" : "",
            ["BrowserStack:AccessKey"] = credentials ? "synthetic-key" : "",
        }).Build();

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TransportFailure_ReturnsUnavailableWithoutRetry(bool cancelled)
    {
        using RecordingHandler handler = new(HttpStatusCode.OK) { Failure = cancelled ? "cancelled" : "transport" };
        using HttpClient client = new(handler);
        Assert.False(await new BrowserStackSessionStatusClient(client, Configuration())
            .UpdateAsync(new string('a', 40), "failed", "synthetic reason"));
        Assert.Equal(1, handler.Calls);
    }

    private sealed class RecordingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        internal int Calls { get; private set; }
        internal string Body { get; private set; } = string.Empty;
        internal string? Url { get; private set; }
        internal HttpMethod? Method { get; private set; }
        internal bool HasAuthorization { get; private set; }
        internal string? Failure { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Assert.True(cancellationToken.CanBeCanceled);
            if (Failure == "cancelled")
                throw new OperationCanceledException();
            if (Failure == "transport")
                throw new HttpRequestException("Synthetic failure");
            Url = request.RequestUri?.AbsoluteUri;
            Method = request.Method;
            HasAuthorization = request.Headers.Authorization?.Scheme == "Basic";
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status);
        }
    }
}
