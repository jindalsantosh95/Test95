using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class AccountLinkingProofMutationTests : IDisposable
{
    private const string IdentityUrl = "https://testresources.virginatlantic.com/account-linking-success";
    private readonly IServiceProvider _serviceProvider = AccountLinkingTestServices.Create(IdentityUrl);

    [Fact]
    public async Task CreateAsync_ForwardsAuthenticatedAccountAndTransaction()
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices");
        SetPrincipal("latrinaselena@example.com");
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        var input = ValidInput();
        var response = await mutation.CreateAsync(input);

        Assert.Equal("proof-value", response.Proof);
        var payload = VAA.EAPI.GraphQL.Common.Serialisation.Json.Deserialize<CreateLinkingProofRequest>(
            await GetRequest("/account-linking-proof").Content!.ReadAsStringAsync());
        Assert.Equal("1501282576", payload.FlyingClubAccountNumber);
        Assert.Equal("transaction", payload.TransactionId);
        Assert.Equal(input.ClientId, payload.ClientId);
        Assert.Equal(input.Scope, payload.Scope);
    }

    [Fact]
    public async Task CreateAsync_RejectsMissingAuthenticatedEmail()
    {
        SetPrincipal(null);
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task CreateAsync_UsesPreferredUsernameWhenEmailIsMissing()
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices");
        SetPrincipal(null, new Claim("preferred_username", "latrinaselena@example.com"));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        Assert.Equal("proof-value", (await mutation.CreateAsync(ValidInput())).Proof);
        var payload = VAA.EAPI.GraphQL.Common.Serialisation.Json.Deserialize<CreateLinkingProofRequest>(
            await GetRequest("/account-linking-proof").Content!.ReadAsStringAsync());
        Assert.Equal("1501282576", payload.FlyingClubAccountNumber);
    }

    [Fact]
    public async Task CreateAsync_PrefersEmailOverPreferredUsername()
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices");
        SetPrincipal("latrinaselena@example.com", new Claim("preferred_username", "other@example.com"));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        Assert.Equal("proof-value", (await mutation.CreateAsync(ValidInput())).Proof);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("Member <member@example.com>")]
    public async Task CreateAsync_RejectsPreferredUsernameThatIsNotAnEmail(string username)
    {
        SetPrincipal(null, new Claim("preferred_username", username));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task CreateAsync_RejectsAmbiguousPreferredUsernames()
    {
        SetPrincipal(null, new Claim("preferred_username", "latrinaselena@example.com"),
            new Claim("preferred_username", "other@example.com"));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task CreateAsync_DoesNotUsePreferredUsernameToResolveConflictingEmails()
    {
        SetPrincipal("latrinaselena@example.com", new Claim("email", "other@example.com"),
            new Claim("preferred_username", "latrinaselena@example.com"));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Theory]
    [InlineData("/api/v1/GraphQL", 128, true)]
    [InlineData("/api/v1/GraphQL", 129, false)]
    [InlineData("/api/v1/accountCreateLinkingProof", 128, true)]
    [InlineData("/api/v1/accountCreateLinkingProof", 129, false)]
    public async Task TransactionIdSizeIsBoundedBeforeLookupOnBothSurfaces(string path, int size, bool accepted)
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices");
        SetPrincipal(accepted ? "latrinaselena@example.com" : null);
        var context = AsyncContext.Current.Context;
        var request = new TestHttpRequestData(context, new Uri("https://eapi.test" + path), method: "POST");
        request.Headers.Add("Authorization", "Bearer " + context.Features.Get<JwtPrincipalFeature>().AccessToken.Token);
        context.Features.Set<HttpRequestData>(request);
        var input = ValidInput();
        input.TransactionId = new string('a', size);
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();
        if (accepted) Assert.Equal("proof-value", (await mutation.CreateAsync(input)).Proof);
        else
        {
            var error = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(input));
            Assert.Contains("transactionId", Assert.Single(error.Errors).Message);
        }
    }

    [Fact]
    public async Task CreateAsync_RejectsRequestThatExceededTransportInspectionLimit()
    {
        SetPrincipal("latrinaselena@example.com");
        AsyncContext.Current.Context.Features.Set(new AccountLinkingProofRequestLimitFeature());
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();
        var error = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
        Assert.Contains("inspection limit", Assert.Single(error.Errors).Message);
    }

    [Theory]
    [InlineData("/api/v1/GraphQL")]
    [InlineData("/api/v1/accountCreateLinkingProof")]
    public async Task CreateAsync_RejectsOversizedTransactionBeforeAccountLookup(string path)
    {
        SetPrincipal(null);
        var context = AsyncContext.Current.Context;
        context.Features.Set<HttpRequestData>(new TestHttpRequestData(context,
            new Uri("https://eapi.test" + path), method: "POST"));
        var input = ValidInput();
        input.TransactionId = new string('x', 129);

        var error = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() =>
            _serviceProvider.GetRequiredService<AccountLinkingProofMutation>().CreateAsync(input));

        Assert.Contains("transactionId", error.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsInactiveResolvedProfile()
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices/inactive");
        SetPrincipal("latrinaselena@example.com");
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task CreateAsync_RejectsProfileWithoutFlyingClubNumber()
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices/missing-member-number");
        SetPrincipal("latrinaselena@example.com");
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task CreateAsync_RejectsMissingTransactionInput()
    {
        SetPrincipal("latrinaselena@example.com");
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_RejectsAmbiguousAuthenticatedEmail()
    {
        SetPrincipal("latrinaselena@example.com", new Claim(ClaimTypes.Email, "other@example.com"));
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();

        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(ValidInput()));
    }

    [Fact]
    public async Task RedeemAsync_ForwardsProofRequest()
    {
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingMutation>();
        var response = await mutation.RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = "proof",
            TransactionId = "transaction", ClientId = "alexa-client", Resource = "vaa-flying-club"
        });

        Assert.Equal("1234567890", response.FlyingClubAccountNumber);
        Assert.Equal("consent", response.ConsentSubject);
        var payload = VAA.EAPI.GraphQL.Common.Serialisation.Json.Deserialize<RedeemLinkingProofRequest>(
            await GetRequest("/account-linking-proof/redeem").Content!.ReadAsStringAsync());
        Assert.Equal("proof", payload.Proof);
        Assert.Equal("transaction", payload.TransactionId);
    }

    [Fact]
    public async Task RedeemAsync_RejectsMissingRequest()
    {
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingMutation>();
        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.RedeemAsync(null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RedeemAsync_RejectsOversizedTokensBeforeSending(bool oversizedProof)
    {
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingMutation>();
        await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = oversizedProof ? new string('a', 8193) : "proof",
            TransactionId = oversizedProof ? "transaction" : new string('a', 129),
            ClientId = "alexa-client", Resource = "vaa-flying-club"
        }));
        Assert.Empty(((HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>())
            .GetResponses($"{IdentityUrl}/account-linking-proof/redeem"));
    }

    [Theory]
    [InlineData("failure")]
    [InlineData("timeout")]
    public async Task CustomerDependencyFailuresAreRetryableAndDoNotCreateProof(string failure)
    {
        var key = VAA.EAPI.GraphQL.Common.Constants.Environment.HttpClientTimeOutInMilliSeconds;
        var previous = ConfigReader.Try(key);
        try
        {
            ConfigReader.Set(key, "100");
            var factory = new HttpClientFactory();
            var services = AccountLinkingTestServices.Create(IdentityUrl, factory);
            SetCustomerSource("https://testresources.virginatlantic.com/account-linking-customer-" + failure);
            SetPrincipal("latrinaselena@example.com");
            var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() =>
                services.GetRequiredService<AccountLinkingProofMutation>().CreateAsync(ValidInput()));
            var error = Assert.Single(exception.Errors);
            Assert.Equal("ACCOUNT_LINKING_DEPENDENCY_UNAVAILABLE", error.Code);
            Assert.Equal(503, error.Extensions!["httpStatusCode"]);
            Assert.Equal(true, error.Extensions["retryable"]);
            Assert.DoesNotContain(factory.GetResponses(IdentityUrl), response =>
                response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith("/account-linking-proof", StringComparison.Ordinal) == true);
        }
        finally { ConfigReader.Set(key, previous); }
    }

    private HttpRequestMessage GetRequest(string path) =>
        ((HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>())
            .GetResponses($"{IdentityUrl}{path}")
            .Single(response => response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith(path, StringComparison.Ordinal) == true)
            .RequestMessage!;

    private static CreateLinkingProofInput ValidInput() => new()
    {
        TransactionId = "transaction", ClientId = "alexa-client", Resource = "vaa-flying-club",
        Scope = "account:read"
    };

    [Theory]
    [InlineData("/api/v1/GraphQL", true)]
    [InlineData("/api/v1/other", false)]
    [InlineData("/api/v1/accountCreateLinkingProof", true)]
    public async Task PlainContractWorksOnBothAgreedSurfaces(string requestPath, bool accepted)
    {
        SetCustomerSource("https://Testresources.virginatlantic.com/dev/microservices");
        SetPrincipal("latrinaselena@example.com");
        var context = AsyncContext.Current.Context;
        var token = context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;
        var request = new TestHttpRequestData(context, new Uri("https://eapi.test" + requestPath), method: "POST");
        request.Headers.Add("Authorization", "Bearer " + token);
        context.Features.Set<HttpRequestData>(request);
        var input = ValidInput();
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();
        if (accepted) Assert.Equal("proof-value", (await mutation.CreateAsync(input)).Proof);
        else await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.CreateAsync(input));
    }

    [Fact]
    public async Task ProofRejectsBearerDifferentFromAuthenticatedToken()
    {
        SetPrincipal("latrinaselena@example.com");
        var context = AsyncContext.Current.Context;
        var request = new TestHttpRequestData(context, new Uri("https://eapi.test/api/v1/accountCreateLinkingProof"), method: "POST");
        request.Headers.Add("Authorization", "Bearer different");
        context.Features.Set<HttpRequestData>(request);
        var mutation = _serviceProvider.GetRequiredService<AccountLinkingProofMutation>();
        var exception = await Record.ExceptionAsync(() => mutation.CreateAsync(ValidInput()));
        Assert.True(exception is UnauthorizedAccessException or HotChocolate.GraphQLException);
    }

    private static string Proof(string path) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: [new Claim("htu", path)]));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RestProofRejectsUntrustedIssuerOrAudience(bool wrongIssuer)
    {
        SetPrincipal("latrinaselena@example.com");
        var context = AsyncContext.Current.Context;
        var principal = context.Features.Get<JwtPrincipalFeature>().Principal;
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: wrongIssuer ? "https://untrusted.test" : "https://identity.test",
            audience: wrongIssuer ? ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.EAPIGraphAppID) : "other-audience"));
        context.Features.Set(new JwtPrincipalFeature(principal, new AuthToken { Token = token }));
        var request = new TestHttpRequestData(context, new Uri("https://eapi.test/api/v1/accountCreateLinkingProof"), method: "POST");
        request.Headers.Add("Authorization", "Bearer " + token);
        context.Features.Set<HttpRequestData>(request);
        var exception = await Record.ExceptionAsync(() => _serviceProvider.GetRequiredService<AccountLinkingProofMutation>().CreateAsync(ValidInput()));
        Assert.True(exception is UnauthorizedAccessException or HotChocolate.GraphQLException);
    }

    private static void SetPrincipal(string? email, params Claim[] additionalClaims)
    {
        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.Name, "named-member"));
        if (email is not null) claims.Add(new Claim("email", email));
        claims.AddRange(additionalClaims);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));
        var context = new TestFunctionContext();
        foreach (var key in new[] { VAA.EAPI.GraphQL.Common.Constants.Environment.AADAuthority,
            VAA.EAPI.GraphQL.Common.Constants.Environment.AADTenant, VAA.EAPI.GraphQL.Common.Constants.Environment.AADTokenV1Issuer,
            VAA.EAPI.GraphQL.Common.Constants.Environment.AB2CAuthority, VAA.EAPI.GraphQL.Common.Constants.Environment.AB2CTenant,
            VAA.EAPI.GraphQL.Common.Constants.Environment.EntraTenant })
            ConfigReader.Set(key, "test");
        ConfigReader.Set(VAA.EAPI.GraphQL.Common.Constants.Environment.IdentityIssuer, "https://identity.test");
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer: "https://identity.test",
            audience: ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.EAPIGraphAppID)));
        context.Features.Set(new JwtPrincipalFeature(principal, new AuthToken { Token = token }));
        var request = new TestHttpRequestData(context, new Uri("https://eapi.test/api/v1/GraphQL"), method: "POST");
        request.Headers.Add("Authorization", "Bearer " + token);
        context.Features.Set<HttpRequestData>(request);
        AsyncContext.Current = new AsyncContext { Context = context };
    }

    private static void SetCustomerSource(string url) =>
        VAA.EAPI.GraphQL.Common.Helper.ConfigReader.Set(
            VAA.EAPI.GraphQL.Common.Constants.Environment.CustomerMsUrl,
            url);

    public void Dispose() => AsyncContext.Current = null;
}
