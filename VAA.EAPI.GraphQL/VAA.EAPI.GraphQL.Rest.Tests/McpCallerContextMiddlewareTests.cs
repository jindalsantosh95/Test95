using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Rest.FunctionHost;
using VAA.EAPI.GraphQL.Rest.Middleware;
using VAA.EAPI.GraphQL.Rest.Middleware.Mcp;
using Xunit;
using Settings = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Rest.Tests;

[Collection(nameof(McpCallerContextMiddlewareTests))]
public sealed class McpCallerContextMiddlewareTests : IDisposable
{
    private readonly Dictionary<string, string?> _original = new();
    private readonly ServiceProvider _services;

    public McpCallerContextMiddlewareTests()
    {
        Set(Settings.McpCallerContextApplicationId, "backend-app");
        Set(Settings.AADTenant, "tenant");
        Set(Settings.AADAuthority, "https://login.example.test");
        Set(Settings.AADTokenV1Issuer, "https://sts.example.test");
        Set(Settings.EAPIGraphAppID, "graphql-api");
        var services = new ServiceCollection();
        _services = services.AddSingleton<IServiceCollection>(services).AddSingleton<Router>()
            .AddSingleton<VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure.AccountLinkingMutation>()
            .AddSingleton<VAA.EAPI.GraphQL.Domain.Account.Queries.FlyingClubAccountSummaryQuery>()
            .AddSingleton<McpCallerContextMiddleware>().BuildServiceProvider();
    }

    [Theory]
    [InlineData("/api/mcp", "POST", "appid", "https://sts.example.test/tenant/")]
    [InlineData("/api/mcp/legacy", "POST", "azp", "https://login.example.test/tenant/v2.0")]
    [InlineData("/api/v1/loyaltyRedeemLinkingProof", "POST", "azp", "https://login.example.test/tenant/v2.0")]
    [InlineData("/api/v1/loyaltyFlyingClubAccountSummary", "GET", "appid", "https://sts.example.test/tenant/")]
    public async Task ValidatedApplicationBearerNeedsNoSecondAssertion(string path, string method, string appClaim, string issuer)
    {
        Assert.True(await Invoke(path: path, method: method, appClaim: appClaim, issuer: issuer));
        // Reusing a valid application bearer is normal OAuth behaviour, not proof redemption.
        Assert.True(await Invoke(path: path, method: method, appClaim: appClaim, issuer: issuer));
    }

    [Theory]
    [InlineData("missing-principal")]
    [InlineData("unauthenticated")]
    [InlineData("wrong-app")]
    [InlineData("missing-app")]
    [InlineData("unconfigured-app")]
    [InlineData("wrong-audience")]
    [InlineData("extra-audience")]
    [InlineData("wrong-issuer")]
    [InlineData("delegated")]
    [InlineData("mismatched-token")]
    [InlineData("missing-header")]
    [InlineData("wrong-role")]
    public async Task InvalidApplicationCannotAccessMarkedOperation(string invalid)
    {
        if (invalid == "unconfigured-app") Set(Settings.McpCallerContextApplicationId, "");
        Assert.False(await Invoke(invalid));
    }

    [Theory]
    [InlineData("/api/v1/version", false)]
    [InlineData("/custom-mcp", true)]
    public async Task UnmarkedRestIsUnchangedAndLegacyMcpHeaderStillWorks(string path, bool mcpHeader)
    {
        Assert.Equal(mcpHeader, await Invoke(path: path, mcpHeader: mcpHeader));
    }

    [Theory]
    [InlineData("/api/mcp", false)]
    [InlineData("/api/mcp/legacy", false)]
    [InlineData("/custom-mcp", true)]
    [InlineData("/api/v1/version", false)]
    public async Task ExistingPublicOperationsDoNotAcquireApplicationAuthentication(string path, bool mcpHeader)
    {
        Set(Settings.McpCallerContextApplicationId, "");
        Assert.True(await Invoke("missing-principal", path: path, mcpHeader: mcpHeader, marked: false));
    }

    [Theory]
    [InlineData("/api/v1/loyaltyFlyingClubAccountSummary", "GET")]
    [InlineData("/api/mcp", "POST")]
    public async Task ExistingApplicationAndBrokerTokenHeaderIsAccepted(string path, string method)
    {
        Assert.True(await Invoke(path: path, method: method, secondary: true));
        Assert.False(await Invoke("wrong-app", path: path, method: method, secondary: true));
        Assert.False(await Invoke("delegated", path: path, method: method, secondary: true));
    }

    [Fact]
    public async Task ExistingBearerWhitespaceNormalizationIsPreserved()
    {
        Assert.True(await Invoke(normalize: true));
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(false, "/")]
    [InlineData(false, "///")]
    [InlineData(true, "")]
    [InlineData(true, "/")]
    [InlineData(true, "///")]
    public async Task ConfiguredIssuerRootTrailingSlashesDoNotRejectCanonicalIssuer(bool v1, string suffix)
    {
        Set(Settings.AADAuthority, "https://login.example.test" + suffix);
        Set(Settings.AADTokenV1Issuer, "https://sts.example.test" + suffix);
        var issuer = v1 ? "https://sts.example.test/tenant/" : "https://login.example.test/tenant/v2.0";
        Assert.True(await Invoke(issuer: issuer));
        Assert.False(await Invoke("wrong-issuer", issuer: issuer));
    }

    private async Task<bool> Invoke(string? invalid = null, string path = "/api/mcp", string method = "POST",
        string appClaim = "azp", string issuer = "https://login.example.test/tenant/v2.0", bool mcpHeader = false, bool marked = true,
        bool secondary = false, bool normalize = false)
    {
        var context = new TestFunctionContext();
        var claims = new List<Claim> { new(ClaimTypes.Role, invalid == "wrong-role" ? "other" : "required") };
        if (invalid != "missing-app") claims.Add(new Claim(appClaim, invalid == "wrong-app" ? "other" : "backend-app"));
        using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var securityKey = new ECDsaSecurityKey(signingKey);
        var jwt = new JwtSecurityToken(invalid == "wrong-issuer" ? "https://untrusted.example.test" : issuer,
            invalid == "wrong-audience" ? "other-api" : "graphql-api", claims, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1),
            new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256));
        if (invalid == "extra-audience") jwt.Payload["aud"] = new[] { "graphql-api", "other-api" };
        if (invalid == "delegated") jwt.Payload["scp"] = "user.read";
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        var request = new TestHttpRequestData(context, new Uri("https://eapi.example.test" + path), new MemoryStream(), method, new());
        if (invalid != "missing-header") request.Headers.TryAddWithoutValidation("Authorization",
            (normalize ? "  Bearer   " : "Bearer ") + token + (normalize ? "  " : "") + (secondary ? ",Token broker-token" : ""));
        // The old header is ignored, including on legacy operations.
        request.Headers.Add("X-EAPI-Caller-Context", "obsolete-header");
        if (mcpHeader) request.Headers.Add("x-mcp-request", "true");
        context.Features.Set<HttpRequestData>(request);
        if (invalid != "missing-principal")
        {
            // Use actual validated/mapped JWT claims, as produced by shared authentication.
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = securityKey, ValidateIssuer = false, ValidateAudience = false
            }, out _);
            if (invalid == "unauthenticated") principal = new ClaimsPrincipal(new ClaimsIdentity(principal.Claims));
            context.Features.Set(new JwtPrincipalFeature(principal,
                new AuthToken { Token = invalid == "mismatched-token" ? "other" : token, OtherToken = secondary ? "broker-token" : null! }));
        }
        var invoked = false;
        await _services.GetRequiredService<McpCallerContextMiddleware>().Invoke(context, _ => { invoked = true; return Task.CompletedTask; });
        Assert.True(invoked); // No new global rejection behaviour for existing calls.
        var tool = new Tool { RequiresAuthentication = marked, RequiresMcpCallerContext = marked, AuthorizedRoles = ["required"] };
        return tool.HasRequiredCallerContext(context.Features.Get<JwtPrincipalFeature>()?.Principal!,
            context.Features.Get<McpCallerContextFeature>()?.Principal!);
    }

    private void Set(string name, string value)
    {
        _original.TryAdd(name, Environment.GetEnvironmentVariable(name));
        ConfigReader.Set(name, value);
    }

    public void Dispose()
    {
        _services.Dispose();
        foreach (var entry in _original) ConfigReader.Set(entry.Key, entry.Value!);
    }
}

[CollectionDefinition(nameof(McpCallerContextMiddlewareTests), DisableParallelization = true)]
public sealed class McpCallerContextMiddlewareCollection;
