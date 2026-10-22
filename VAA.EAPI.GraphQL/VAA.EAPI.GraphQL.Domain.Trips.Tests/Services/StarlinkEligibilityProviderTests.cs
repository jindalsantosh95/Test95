using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services;

[Collection("Flight Ops configuration")]
public sealed class StarlinkEligibilityProviderTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly StarlinkEligibilityProvider _provider;

    public StarlinkEligibilityProviderTests(HttpClientFactoryFixture fixture)
    {
        Setup.CreateTestEnvironmentSettings();
        var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, fixture.Current);
        _provider = new StarlinkEligibilityProvider(connector);
    }

    [Fact]
    public async Task GetEligibilityAsync_ReturnsCategoryResultForMatchingTail()
    {
        var result = await _provider.GetEligibilityAsync(["G-VTOM"]);

        Assert.True(result["G-VTOM"] == true);
    }

    [Fact]
    public async Task GetEligibilityAsync_ReturnsFalseForIneligibleTail()
    {
        var result = await _provider.GetEligibilityAsync(["G-VWHO"]);

        Assert.False(result["G-VWHO"] == true);
    }

    [Fact]
    public async Task GetEligibilityAsync_MapsMultipleTailNumbersFromOneResponse()
    {
        var result = await _provider.GetEligibilityAsync(["G-VTOM", "G-VWHO"]);

        Assert.True(result["G-VTOM"] == true);
        Assert.False(result["G-VWHO"] == true);
    }

    [Fact]
    public async Task GetEligibilityAsync_ReturnsNullForMissingTail()
    {
        var result = await _provider.GetEligibilityAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEligibilityAsync_ReturnsFalseForNegativeCategoryResponse()
    {
        ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-false");

        var result = await _provider.GetEligibilityAsync(["G-VTOM"]);

        Assert.False(result["G-VTOM"] == true);
    }

    [Fact]
    public async Task GetEligibilityAsync_PropagatesServiceFailureToCaller()
    {
        ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-error");

        await Assert.ThrowsAsync<HttpRequestException>(() => _provider.GetEligibilityAsync(["G-VTOM"]));
    }
}
