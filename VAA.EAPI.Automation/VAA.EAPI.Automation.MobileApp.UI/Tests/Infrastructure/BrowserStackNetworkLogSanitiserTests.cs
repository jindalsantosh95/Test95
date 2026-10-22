using System.Net;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class BrowserStackNetworkLogSanitiserTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void NetworkDiagnostics_TC01_GraphQlError_ReturnsOnlySafeMetadata()
    {
        string har = """
            {
              "log": {
                "entries": [{
                  "request": {
                    "method": "POST",
                    "url": "https://example.test/digital/graphql?token=secret",
                    "headers": [{"name":"x-correlation-id","value":"correlation-123"}],
                    "postData": {"text":"{\"operationName\":\"bookCreateOrder\",\"variables\":{\"card\":\"4111111111111111\"}}"}
                  },
                  "response": {
                    "status": 200,
                    "content": {"text":"{\"errors\":[{\"message\":\"Sensitive passenger detail\",\"extensions\":{\"code\":\"ORDER_FAILED\"}}]}"}
                  }
                }]
              }
            }
            """;

        string summary = Assert.Single(BrowserStackNetworkLogSanitiser.Summarise(har));

        Assert.Contains("operation=bookCreateOrder", summary, StringComparison.Ordinal);
        Assert.Contains("backendCodes=ORDER_FAILED", summary, StringComparison.Ordinal);
        Assert.Contains("request-correlation=x-correlation-id:correlation-123", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("Sensitive passenger detail", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("4111111111111111", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("token", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", summary, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void NetworkDiagnostics_TC02_SuccessfulGraphQlResponse_IsNotReported()
    {
        string har = """
            {"log":{"entries":[{"request":{"method":"POST","url":"https://example.test/graphql"},"response":{"status":200,"content":{"text":"{\"data\":{\"ok\":true}}"}}}]}}
            """;

        IReadOnlyList<string> summaries = BrowserStackNetworkLogSanitiser.Summarise(har);

        Assert.Empty(summaries);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public async Task NetworkDiagnostics_TC03_MalformedNetworkPayload_ReturnsNoSummaries()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BrowserStack:Username"] = "test-user",
                ["BrowserStack:AccessKey"] = "test-key",
            })
            .Build();
        using HttpClient httpClient = new(new MalformedNetworkLogHandler());
        BrowserStackNetworkLogClient client = new(httpClient, config);

        IReadOnlyList<string> summaries = await client.GetSafeFailureSummariesAsync("session-id");

        Assert.Empty(summaries);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void NetworkDiagnostics_TC04_ResponseCorrelation_PreservesSourceAndRejectsInvalidEapiIdentifiers()
    {
        string har = """
            {"log":{"entries":[{
              "request":{"method":"POST","url":"https://example.test/graphql",
                "headers":[{"name":"x-request-id","value":"request-123"}]},
              "response":{"status":500,"headers":[
                {"name":"API-Correlation-ID","value":"11111111-2222-3333-4444-555555555555"},
                {"name":"api-operation-id","value":"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"},
                {"name":"api-session-id","value":"sensitive-invalid-value"},
                {"name":"Authorization","value":"secret"}]}
            }]}}
            """;

        string summary = Assert.Single(BrowserStackNetworkLogSanitiser.Summarise(har));

        Assert.Contains("request-correlation=x-request-id:request-123", summary, StringComparison.Ordinal);
        Assert.Contains("response-correlation=api-correlation-id:11111111-2222-3333-4444-555555555555", summary, StringComparison.Ordinal);
        Assert.Contains("api-operation-id:aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-invalid-value", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", summary, StringComparison.Ordinal);
    }

    private sealed class MalformedNetworkLogHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            bool sessionRequest = request.RequestUri?.AbsolutePath.EndsWith(
                ".json",
                StringComparison.Ordinal) == true;
            string content = sessionRequest
                ? "{\"automation_session\":{\"build_hashed_id\":\"build-id\"}}"
                : "<html>temporary BrowserStack error</html>";
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(content),
            };
            return Task.FromResult(response);
        }
    }
}
