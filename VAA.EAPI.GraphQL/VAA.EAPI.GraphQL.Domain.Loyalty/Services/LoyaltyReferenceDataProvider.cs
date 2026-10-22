using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ReferenceData;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services;

public sealed class LoyaltyReferenceDataProvider : ILoyaltyReferenceDataProvider
{
    private readonly IHttpConnector _connector;

    public LoyaltyReferenceDataProvider(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public Task<IEnumerable<AirportInfo>> GetAirports() => Get<AirportInfo>("airportdetail");

    public Task<IEnumerable<AirlineInfo>> GetAirlines() => Get<AirlineInfo>("airlinedetails");

    public Task<IEnumerable<BlacklistedEmailList>> GetBlacklistedEmails() => Get<BlacklistedEmailList>("blackListedEmails");

    private async Task<IEnumerable<T>> Get<T>(string resource)
    {
        var uri = string.Format(ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl), resource);
        var headers = new Dictionary<string, string>
        {
            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) }
        };

        return (await _connector.GetAsync<IEnumerable<T>>(uri, headers, string.Empty)).Response;
    }
}
