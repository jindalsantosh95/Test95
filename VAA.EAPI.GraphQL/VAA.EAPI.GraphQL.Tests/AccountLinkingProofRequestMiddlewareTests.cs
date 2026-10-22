using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Middleware;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Tests
{
    [Collection(nameof(AccountLinkingProofRequestMiddlewareTests))]
    public sealed class AccountLinkingProofRequestMiddlewareTests : IDisposable
    {
        private const string Authority = "https://login.example.test";
        private const string Tenant = "customer";
        private const string Audience = "eapi-account-linking";
        private readonly Dictionary<string, string?> _original = new();

        public AccountLinkingProofRequestMiddlewareTests()
        {
            Set("AAD:Authority", Authority);
            Set("AAD:TenantId", Tenant);
            Set("AAD:TokenV1Issuer", Authority);
            Set("EAPIGraph:AppID", Audience);
            Set("AB2C:Authority", "https://b2c.example.test");
            Set("AB2C:TenantId", "customer.onmicrosoft.com");
            Set("AB2C:AB2CAudience", "customer-client");
            Set("AAD:EntraTenantId", "customer");
            Set("Identity:Issuer", "https://identity.example.test");
        }

        [Fact]
        public async Task MissingAuthorizationReachesOperationAuthorizationGate()
        {
            var context = CreateContext(ProofRequest());

            Assert.True(await InvokeAsync(context));
        }

        [Fact]
        public async Task ValidatedSingleBearerAndExactOperationAreAccepted()
        {
            var token = Token(Audience);
            var context = CreateContext(ProofRequest(), $"Bearer {token}");
            context.Features.Set(new JwtPrincipalFeature(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "customer")], "Bearer")),
                new AuthToken { Token = token }));

            Assert.True(await InvokeAsync(context));
        }

        [Theory]
        [InlineData(Audience, true)]
        [InlineData("another-api", false)]
        public async Task IdentityIssuedTokenRequiresGraphAudience(string audience, bool accepted)
        {
            var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: "https://identity.example.test", audience: audience,
                expires: DateTime.UtcNow.AddMinutes(1)));
            var context = CreateContext(ProofRequest(), $"Bearer {token}");
            context.Features.Set(new JwtPrincipalFeature(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "customer")], "Bearer")),
                new AuthToken { Token = token }));

            Assert.Equal(accepted, await InvokeAsync(context));
        }

        [Theory]
        [InlineData("Bearer {token},Bearer second")]
        [InlineData("Basic {token}")]
        [InlineData("Bearer {token} trailing")]
        public async Task PresentInvalidCredentialGrammarIsRejected(string authorization)
        {
            var token = Token(Audience);
            var context = CreateContext(ProofRequest(), authorization.Replace("{token}", token, StringComparison.Ordinal));
            context.Features.Set(new JwtPrincipalFeature(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "customer")], "Bearer")),
                new AuthToken { Token = token }));

            Assert.False(await InvokeAsync(context));
        }

        [Fact]
        public async Task TrustedSignatureContextWithWrongAudienceIsRejected()
        {
            var token = Token("another-api");
            var context = CreateContext(ProofRequest(), $"Bearer {token}");
            context.Features.Set(new JwtPrincipalFeature(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "customer")], "Bearer")),
                new AuthToken { Token = token }));

            Assert.False(await InvokeAsync(context));
        }

        [Fact]
        public async Task AliasedProofFieldIsRejectedBeforeExecution()
        {
            var context = CreateContext("{\"operationName\":\"Create\",\"query\":\"mutation Create { alias: accountCreateLinkingProof(input: { transactionAssertion: \\\"a\\\", transactionProof: \\\"b\\\" }) { linkingProof } }\"}");

            Assert.False(await InvokeAsync(context));
        }

        [Fact]
        public async Task ExistingTwoCredentialOperationRemainsUntouched()
        {
            var context = CreateContext("{\"operationName\":\"Booking\",\"query\":\"query Booking { booking { id } }\"}", "Bearer first,Token second");

            Assert.True(await InvokeAsync(context));
        }

        [Fact]
        public async Task NonSeekableRequestBodyIsBufferedForDownstreamProcessing()
        {
            var context = new TestFunctionContext();
            var request = new NonSeekableHttpRequestData(context, ProofRequest());
            context.Features.Set<HttpRequestData>(request);
            var downstreamBody = string.Empty;

            await new AccountLinkingProofRequestMiddleware().Invoke(context, async downstreamContext =>
            {
                var downstreamRequest = await downstreamContext.GetHttpRequestDataAsync();
                using var reader = new StreamReader(downstreamRequest!.Body, Encoding.UTF8, leaveOpen: true);
                downstreamBody = await reader.ReadToEndAsync();
            });

            Assert.Equal(ProofRequest(), downstreamBody);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(70000)]
        public async Task NonSeekableUnrelatedRequestReachesUnchangedGetData(int padding)
        {
            var services = new ServiceCollection().AddLogging();
            services.AddGraphQLServer().AddQueryType(descriptor => descriptor.Name("Query")
                .Field("status").Type<HotChocolate.Types.StringType>().Resolve(_ => ValueTask.FromResult<object?>("ok")));
            services.AddSingleton<Gateway>().AddSingleton<global::VAA.EAPI.GraphQL.GraphQL>()
                .AddSingleton<AccountLinkingProofRequestMiddleware>();
            using var serviceProvider = services.BuildServiceProvider();
            var context = new TestFunctionContext();
            AsyncContext.Current = new AsyncContext { Context = context };
            var body = new string(' ', padding) + "{\"query\":\"query UnrelatedRequestCompatibilityThroughTheExistingGateway { status }\"}";
            context.Features.Set<HttpRequestData>(new NonSeekableHttpRequestData(context, body));
            await serviceProvider.GetRequiredService<AccountLinkingProofRequestMiddleware>().Invoke(context, async downstream =>
            {
                var request = await downstream.GetHttpRequestDataAsync();
                var memory = Assert.IsAssignableFrom<MemoryStream>(request!.Body);
                Assert.Equal(body, Encoding.UTF8.GetString(memory.ToArray()));
                Assert.Equal(body, Encoding.UTF8.GetString(memory.ToArray()));
                var response = await serviceProvider.GetRequiredService<global::VAA.EAPI.GraphQL.GraphQL>().GetData(request, downstream);
                response.Body.Position = 0;
                using var reader = new StreamReader(response.Body);
                var result = await reader.ReadToEndAsync();
                Assert.DoesNotContain("errors", result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("ok", result, StringComparison.Ordinal);
            });
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public async Task LargeRequestsPreserveBodyAndMarkProofExecutionForRejection(bool seekable, bool proof)
        {
            var body = new string(' ', 70000) + (proof ? ProofRequest() : "{\"query\":\"query { booking { id } }\"}");
            var context = CreateContext(body);
            if (!seekable)
                context.Features.Set<HttpRequestData>(new NonSeekableHttpRequestData(context, body));
            var originalRequest = await context.GetHttpRequestDataAsync();
            var invoked = false;
            await new AccountLinkingProofRequestMiddleware().Invoke(context, async downstreamContext =>
            {
                invoked = true;
                Assert.True(originalRequest!.Body.Position <= VAA.EAPI.GraphQL.Common.AccountLinkingProofRequestLimitFeature.MaximumInspectionBytes + 1);
                Assert.NotNull(downstreamContext.Features.Get<VAA.EAPI.GraphQL.Common.AccountLinkingProofRequestLimitFeature>());
                var request = await downstreamContext.GetHttpRequestDataAsync();
                using var reader = new StreamReader(request!.Body, Encoding.UTF8, leaveOpen: true);
                Assert.Equal(body, await reader.ReadToEndAsync());
            });
            Assert.True(invoked);
        }

        [Theory]
        [InlineData("[null, 1, {\"query\":\"query { booking { id } }\"}]")]
        [InlineData("[{\"query\":\"query {\"}]")]
        [InlineData("[{\"query\":{}}]")]
        [InlineData("{\"pqId\":\"../unrelated\"}")]
        [InlineData("{\"pqId\":\"missing-unrelated-query\"}")]
        [InlineData("{\"pqId\":\"unrelated\",\"query\":\"query { booking { id } }\"}")]
        public async Task UnrelatedInvalidRequestsRetainExistingHandlerAndBody(string body)
        {
            var context = CreateContext(body, "Bearer first,Token second");
            Assert.True(await InvokeAsync(context));
            var request = await context.GetHttpRequestDataAsync();
            using var reader = new StreamReader(request!.Body);
            Assert.Equal(body, await reader.ReadToEndAsync());
        }

        [Theory]
        [InlineData("null")]
        [InlineData("{\"query\":\"query {\"}")]
        [InlineData("{\"query\":{}}")]
        public async Task MalformedBatchEntryDoesNotHideLaterProof(string entry)
        {
            Assert.False(await InvokeAsync(CreateContext($"[{entry},{ProofRequest()}]")));
        }

        [Fact]
        public async Task ConflictingPersistedAndInlineProofIsStillRejected()
        {
            var body = ProofRequest().Insert(1, "\"pqId\":\"unrelated\",");
            Assert.False(await InvokeAsync(CreateContext(body)));
        }

        [Fact]
        public async Task UrlPersistedProofCannotHideBehindUnrelatedInlineQuery()
        {
            var store = Directory.CreateTempSubdirectory("proof-query-test-").FullName;
            var queryFile = Path.Combine(store, "proof.graphql");
            var setting = VAA.EAPI.GraphQL.Common.Constants.Environment.PersistedQueryStore;
            var original = System.Environment.GetEnvironmentVariable(setting);
            try
            {
                File.WriteAllText(queryFile, "mutation Create { accountCreateLinkingProof { linkingProof } }");
                VAA.EAPI.GraphQL.Common.Helper.ConfigReader.Set(setting, store);
                var context = CreateContext("{\"query\":\"query { booking { id } }\"}", url: "https://graphql.example.test/api/v1/GraphQL?pqId=proof");
                Assert.False(await InvokeAsync(context));
            }
            finally
            {
                VAA.EAPI.GraphQL.Common.Helper.ConfigReader.Set(setting, original!);
                File.Delete(queryFile);
                Directory.Delete(store);
            }
        }

        [Fact]
        public async Task MixedCaseProofEnvelopeStillEnforcesShape()
        {
            var body = ProofRequest().Replace("\"query\"", "\"Query\"").Replace("\"operationName\"", "\"OperationName\"");
            Assert.True(await InvokeAsync(CreateContext(body)));
            Assert.False(await InvokeAsync(CreateContext(body.Replace("accountCreateLinkingProof", "alias: accountCreateLinkingProof"))));
        }

        [Theory]
        [InlineData("\"Query\":\"query { booking { id } }\",")]
        [InlineData("\"OperationName\":\"Other\",")]
        [InlineData("\"PqId\":\"missing\",")]
        public async Task AmbiguousOrConflictingProofEnvelopeIsRejected(string extraProperty)
        {
            Assert.False(await InvokeAsync(CreateContext(ProofRequest().Insert(1, extraProperty))));
        }

        [Fact]
        public async Task TokenBindingIsCaseSensitive()
        {
            var token = Token(Audience);
            var context = CreateContext(ProofRequest(), $"Bearer {token}");
            context.Features.Set(new JwtPrincipalFeature(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "customer")], "Bearer")),
                new AuthToken { Token = token.ToUpperInvariant() }));
            Assert.False(await InvokeAsync(context));
        }

        private static async Task<bool> InvokeAsync(TestFunctionContext context)
        {
            var invoked = false;
            await new AccountLinkingProofRequestMiddleware().Invoke(context, _ =>
            {
                invoked = true;
                return Task.CompletedTask;
            });
            return invoked;
        }

        private static TestFunctionContext CreateContext(string body, string? authorization = null, string url = "https://graphql.example.test/api/v1/GraphQL")
        {
            var context = new TestFunctionContext();
            var request = new TestHttpRequestData(
                context,
                new Uri(url),
                new MemoryStream(Encoding.UTF8.GetBytes(body)),
                "POST",
                new Dictionary<string, string>());
            if (authorization is not null)
                request.Headers.TryAddWithoutValidation("authorization", authorization);
            context.Features.Set<HttpRequestData>(request);
            return context;
        }

        private static string ProofRequest() =>
            "{\"operationName\":\"Create\",\"query\":\"mutation Create { accountCreateLinkingProof(input: { transactionAssertion: \\\"a\\\", transactionProof: \\\"b\\\" }) { linkingProof } }\"}";

        private static string Token(string audience) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: $"{Authority}/{Tenant}/v2.0",
            audience: audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, "customer")],
            expires: DateTime.UtcNow.AddMinutes(1)));

        private sealed class NonSeekableHttpRequestData : HttpRequestData
        {
            private readonly Stream _body;

            public NonSeekableHttpRequestData(FunctionContext context, string body) : base(context)
            {
                _body = new NonSeekableReadStream(Encoding.UTF8.GetBytes(body));
            }

            public override Uri Url => new("https://graphql.example.test/api/v1/GraphQL");
            public override Stream Body => _body;
            public override HttpHeadersCollection Headers { get; } = [];
            public override IReadOnlyCollection<IHttpCookie> Cookies => [];
            public override IEnumerable<ClaimsIdentity> Identities => [];
            public override string Method => "POST";
            public override HttpResponseData CreateResponse() => new TestHttpResponseData(FunctionContext);
        }

        private sealed class NonSeekableReadStream(byte[] body) : MemoryStream(body)
        {
            public override bool CanSeek => false;
            public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
            public override long Position
            {
                get => base.Position;
                set => throw new NotSupportedException();
            }
        }

        private void Set(string name, string value)
        {
            _original[name] = System.Environment.GetEnvironmentVariable(name);
            System.Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            AsyncContext.Current = null;
            foreach (var setting in _original)
                System.Environment.SetEnvironmentVariable(setting.Key, setting.Value);
        }
    }

    [CollectionDefinition(nameof(AccountLinkingProofRequestMiddlewareTests), DisableParallelization = true)]
    public sealed class AccountLinkingProofRequestMiddlewareCollection;
}
