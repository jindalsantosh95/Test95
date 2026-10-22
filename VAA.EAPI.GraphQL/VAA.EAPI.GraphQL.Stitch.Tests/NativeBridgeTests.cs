using System.Net;
using System.Security.Claims;
using System.Text;
using HotChocolate.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;
using VAA.EAPI.GraphQL.Stitch.Core.WebSockets;
using Xunit;

namespace VAA.EAPI.GraphQL.Stitch.Tests
{
    [Collection("Stitch configuration")]
    public sealed class NativeBridgeTests
    {
        [Theory]
        [InlineData(false, HttpStatusCode.Unauthorized)]
        [InlineData(true, HttpStatusCode.OK)]
        public async Task RequiresBridgeCredential(bool authorized, HttpStatusCode expected)
        {
            var oldSecret = ConfigReader.Try("NativeWebSocketsBridgeSecret");
            var secret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            try
            {
                ConfigReader.Set("NativeWebSocketsBridgeSecret", secret);
                var services = new ServiceCollection();
                services.AddGraphQL();
                await using var provider = services.BuildServiceProvider();
                using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
                var context = new TestContext { InstanceServices = provider };
                var request = new TestRequest(context, "{\"command\":\"init\",\"connectionId\":\"" + Guid.NewGuid() + "\"}");
                if (authorized) { request.Headers.Add("X-Native-WebSocket-Bridge", secret); }
                var response = await new NativeGraphQLBridge(registry).RunAsync(request, context);
                Assert.Equal(expected, response.StatusCode);
                Assert.Equal("no-store", response.Headers.GetValues("Cache-Control").Single());
            }
            finally
            {
                ConfigReader.Set("NativeWebSocketsBridgeSecret", oldSecret);
            }
        }

        private sealed class TestContext : FunctionContext
        {
            public override string InvocationId => "test";
            public override string FunctionId => "bridge";
            public override TraceContext TraceContext => null!;
            public override BindingContext BindingContext => null!;
            public override RetryContext RetryContext => null!;
            public override IServiceProvider InstanceServices { get; set; } = null!;
            public override FunctionDefinition FunctionDefinition => null!;
            public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
            public override IInvocationFeatures Features => null!;
        }

        private sealed class TestRequest(FunctionContext context, string body) : HttpRequestData(context)
        {
            public override Stream Body { get; } = new MemoryStream(Encoding.UTF8.GetBytes(body));
            public override HttpHeadersCollection Headers { get; } = new();
            public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();
            public override Uri Url => new("http://localhost/api/internal/graphql-websocket");
            public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();
            public override string Method => "POST";
            public override HttpResponseData CreateResponse() => new TestResponse(FunctionContext);
        }

        private sealed class TestResponse(FunctionContext context) : HttpResponseData(context)
        {
            public override HttpStatusCode StatusCode { get; set; }
            public override HttpHeadersCollection Headers { get; set; } = new();
            public override Stream Body { get; set; } = new MemoryStream();
            public override HttpCookies Cookies => null!;
        }
    }

    [CollectionDefinition("Stitch configuration", DisableParallelization = true)]
    public sealed class StitchConfigurationCollection { }
}
