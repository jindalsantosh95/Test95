using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests;

public sealed class LoyaltyReferenceDataProviderTests
{
    private readonly HttpClientFactory _httpClientFactory = new();
    private readonly LoyaltyReferenceDataProvider _provider;

    public LoyaltyReferenceDataProviderTests()
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://testurl.api.com/{0}");
        var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, _httpClientFactory);
        _provider = new LoyaltyReferenceDataProvider(connector);
    }

    [Fact]
    public void ConstructorRejectsNullConnector() => Assert.Throws<ArgumentNullException>(() => new LoyaltyReferenceDataProvider(null));

    [Fact]
    public async Task RetrievesLoyaltyReferenceDataFromExpectedResources()
    {
        var airports = await _provider.GetAirports();
        var airlines = await _provider.GetAirlines();
        var blacklistedEmails = await _provider.GetBlacklistedEmails();

        Assert.Equal("LHR", Assert.Single(airports).AirportCode);
        Assert.Equal("VS", Assert.Single(airlines).CarrierCode);
        Assert.Contains("blocked@example.test", Assert.Single(blacklistedEmails).Emails);

        var requests = _httpClientFactory.GetResponses("https://testurl.api.com").Select(response => response.RequestMessage).ToList();
        Assert.Contains(requests, request => request.RequestUri.AbsolutePath == "/airportdetail");
        Assert.Contains(requests, request => request.RequestUri.AbsolutePath == "/airlinedetails");
        Assert.Contains(requests, request => request.RequestUri.AbsolutePath == "/blackListedEmails");
        Assert.All(requests, request => Assert.True(request.Headers.Contains(Common.Constants.Application.OcpKeyHeader)));
    }
}
