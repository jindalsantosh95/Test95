using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubTokenValidationHandlerTests
{
    private readonly IServiceProvider _serviceProvider;

    public FlyingClubTokenValidationHandlerTests()
    {
        ConfigReader.Set(Environment.FlyingClubTokenKeyId, "https://test.vault.azure.net/keys/flying-club/test");
        ConfigReader.Set(Environment.FlyingClubTokenAuthorizedParty, "graphql-application-id");
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IFlyingClubTokenValidator, KeyVaultFlyingClubTokenValidator>()
            .AddSingleton<IFlyingClubTokenValidationHandler, FlyingClubTokenValidationHandler>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("wrong-application")]
    public async Task HandleAsync_RejectsMissingOrDifferentAuthenticatedCaller(string? caller)
    {
        var handler = _serviceProvider.GetRequiredService<IFlyingClubTokenValidationHandler>();
        var result = await handler.HandleAsync(new FlyingClubTokenValidationRequest
        {
            AccessToken = "signed.jwt.value",
            AuthenticatedCallerApplicationId = caller
        }, TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.Unauthorized, result.Value.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("signed.jwt.value")]
    public async Task HandleAsync_ReturnsUnauthorized_WhenTokenCannotBeValidated(string? token)
    {
        var handler = _serviceProvider.GetRequiredService<IFlyingClubTokenValidationHandler>();

        var result = await handler.HandleAsync(
            new FlyingClubTokenValidationRequest
            {
                AccessToken = token,
                AuthenticatedCallerApplicationId = "graphql-application-id"
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error : null);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.Unauthorized, result.Value.Status);
    }

}
