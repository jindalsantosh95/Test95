using Microsoft.Extensions.DependencyInjection;
using Azure.Security.KeyVault.Keys.Cryptography;
using System.Security.Cryptography;
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubTokenHandlerTests
{
    private readonly IServiceProvider _serviceProvider;

    public FlyingClubTokenHandlerTests()
    {
        ConfigReader.Set(Environment.FlyingClubTokenKeyId, "https://test.vault.azure.net/keys/flying-club/test");
        ConfigReader.Set(Environment.FlyingClubTokenIssuer, "https://identity.test/flying-club");
        ConfigReader.Set(Environment.FlyingClubTokenAudience, "flying-club-profile");
        ConfigReader.Set(Environment.FlyingClubTokenClaimType, "fc_number");
        ConfigReader.Set(Environment.FlyingClubTokenAuthorizedParty, "graphql-application-id");
        ConfigReader.Set(Environment.FlyingClubTokenLifetimeSeconds, "300");
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IFlyingClubTokenSigner, KeyVaultFlyingClubTokenSigner>()
            .AddSingleton<IFlyingClubTokenHandler, FlyingClubTokenHandler>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task HandleAsync_BindsOrdinaryTokenToAuthenticatedCaller()
    {
        using var rsa = RSA.Create(2048);
        var client = new CryptographyClient(new Azure.Security.KeyVault.Keys.JsonWebKey(rsa, includePrivateParameters: true)
        {
            Id = ConfigReader.Get(Environment.FlyingClubTokenKeyId)
        });
        var constructor = typeof(KeyVaultFlyingClubTokenSigner).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(CryptographyClient)], null)!;
        using var services = new ServiceCollection().AddLogging().AddSingleton(TimeProvider.System)
            .AddSingleton<IFlyingClubTokenSigner>(_ => (KeyVaultFlyingClubTokenSigner)constructor.Invoke([client]))
            .AddSingleton<IFlyingClubTokenHandler, FlyingClubTokenHandler>()
            .BuildServiceProvider();
        var handler = services.GetRequiredService<IFlyingClubTokenHandler>();

        var result = await handler.HandleAsync(
            new FlyingClubTokenRequest
            {
                FlyingClubAccountNumber = "1234567890",
                CallerApplicationId = "graphql-application-id",
                AuthenticatedCallerApplicationId = "graphql-application-id"
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, result.Value.Status);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value.Value.AccessToken);
        Assert.Equal("graphql-application-id", jwt.Payload[Constants.Application.AuthorizedPartyClaim]);
        Assert.False(jwt.Payload.ContainsKey("token_use"));
    }

    [Fact]
    public async Task HandleAsync_ReturnsForbidden_WhenRequestedCallerDoesNotMatchAuthenticatedCaller()
    {
        var handler = _serviceProvider.GetRequiredService<IFlyingClubTokenHandler>();

        var result = await handler.HandleAsync(
            new FlyingClubTokenRequest
            {
                FlyingClubAccountNumber = "1234567890",
                CallerApplicationId = "different-application-id",
                AuthenticatedCallerApplicationId = "graphql-application-id"
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.Forbidden, result.Value.Status);
        Assert.Null(result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABC123")]
    [InlineData("123456789012345678901")]
    public async Task HandleAsync_ReturnsBadRequest_WhenAccountNumberIsInvalid(string? accountNumber)
    {
        var handler = _serviceProvider.GetRequiredService<IFlyingClubTokenHandler>();

        var result = await handler.HandleAsync(
            new FlyingClubTokenRequest { FlyingClubAccountNumber = accountNumber },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
    }

}
