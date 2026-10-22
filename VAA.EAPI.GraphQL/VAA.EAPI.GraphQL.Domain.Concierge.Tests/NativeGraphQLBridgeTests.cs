using Microsoft.Extensions.DependencyInjection;
using HotChocolate.Execution;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using VAA.EAPI.GraphQL.Triggers.Http;
using VAA.EAPI.GraphQL.WebSocketHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    [Collection("Native transport configuration")]
    public sealed class NativeGraphQLBridgeTests
    {
        [Theory]
        [InlineData("read", null)]
        [InlineData("read", "null")]
        [InlineData("read", "invalid")]
        [InlineData("stop", null)]
        [InlineData("stop", "null")]
        [InlineData("stop", "invalid")]
        public async Task MalformedSessionReturnsBadRequestAsync(string command, string? session)
        {
            var body = new JObject { ["command"] = command, ["connectionId"] = Guid.NewGuid() };
            if (session is not null) { body["sessionId"] = session == "null" ? JValue.CreateNull() : session; }
            Assert.Equal(HttpStatusCode.BadRequest, await InvokeAsync(body));
        }

        [Theory]
        [InlineData(null, false, false, HttpStatusCode.OK)]
        [InlineData("Bearer unreadable", false, false, HttpStatusCode.Unauthorized)]
        [InlineData("", false, false, HttpStatusCode.Unauthorized)]
        [InlineData("Bearer accepted", true, false, HttpStatusCode.OK)]
        [InlineData("Bearer broker", false, true, HttpStatusCode.OK)]
        public async Task InitializationRequiresAcceptedFeatureWhenCredentialsSuppliedAsync(
            string? authorization, bool authenticated, bool broker, HttpStatusCode expected)
        {
            var body = new JObject { ["command"] = "init", ["connectionId"] = Guid.NewGuid() };
            Assert.Equal(expected, await InvokeAsync(body, authorization, authenticated, broker));
        }

        [Theory]
        [InlineData("@Microsoft.KeyVault(SecretUri=https://example.vault.azure.net/secrets/bridge)")]
        [InlineData("  @microsoft.keyvault(VaultName=example;SecretName=bridge)")]
        [InlineData("                                ")]
        public async Task UnresolvedOrBlankBridgeCredentialIsRejectedEvenWhenItMatchesAsync(string secret)
        {
            var body = new JObject { ["command"] = "init", ["connectionId"] = Guid.NewGuid() };
            Assert.Equal(HttpStatusCode.Unauthorized, await InvokeAsync(body, configuredSecret: secret));
        }

        [Theory]
        [InlineData("@Microsoft.KeyVault(SecretUri=https://example.vault.azure.net/secrets/bridge)", false)]
        [InlineData("  @microsoft.keyvault(VaultName=example;SecretName=bridge)", false)]
        [InlineData("                                ", false)]
        [InlineData("test-only-resolved-credential-32-characters", true)]
        public void TransportStartupRequiresResolvedBridgeCredential(string secret, bool accepted)
        {
            var settings = new Dictionary<string, string>
            {
                ["NativeWebSocketsRedisPrefix"] = "graphql-test",
                ["FUNCTIONS_WORKER_PROCESS_COUNT"] = "1",
                ["NativeWebSocketsBridgeSecret"] = secret
            };
            var previous = settings.Keys.ToDictionary(key => key, key => ConfigReader.Try(key));
            try
            {
                foreach (var setting in settings) { ConfigReader.Set(setting.Key, setting.Value); }
                if (accepted)
                {
                    Assert.Equal(secret, TransportOptions.FromEnvironment().BridgeSecret);
                }
                else
                {
                    Assert.Throws<InvalidOperationException>(() => TransportOptions.FromEnvironment());
                }
            }
            finally
            {
                foreach (var setting in previous) { ConfigReader.Set(setting.Key, setting.Value); }
            }
        }

        private static async Task<HttpStatusCode> InvokeAsync(JObject body, string? authorization = null, bool authenticated = false, bool broker = false, string? configuredSecret = null)
        {
            var previousSecret = ConfigReader.Try("NativeWebSocketsBridgeSecret");
            var secret = configuredSecret ?? Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            ConfigReader.Set("NativeWebSocketsBridgeSecret", secret);
            try
            {
                await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
                using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
                var context = new TestFunctionContext();
                if (authenticated) { context.Features.Set(new JwtPrincipalFeature(NativeSubscriptionTests.Caller(), new AuthToken())); }
                if (broker) { context.Features.Set(new FlyingClubTokenFeature("untrusted-broker-token")); }
                var headers = new Dictionary<string, string> { ["X-Native-WebSocket-Bridge"] = secret };
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body.ToString()));
                var request = new TestHttpRequestData(context, new Uri("http://localhost/api/internal/graphql-websocket"), stream, "POST", headers);
                if (authorization is not null) { request.Headers.TryAddWithoutValidation("Authorization", authorization); }
                var response = await new NativeGraphQLBridge(registry).RunAsync(request, context);
                return response.StatusCode;
            }
            finally
            {
                ConfigReader.Set("NativeWebSocketsBridgeSecret", previousSecret);
            }
        }
    }
}
