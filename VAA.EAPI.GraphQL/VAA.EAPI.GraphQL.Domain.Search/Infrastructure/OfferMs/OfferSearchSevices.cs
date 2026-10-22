using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.OfferMs;

public class OfferSearchSevices : IOfferSearchServices
{
    private static readonly Dictionary<string, string> _headers = new()
    {
        {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.SearchOfferMsOcpKey)},
        {Common.Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.SearchOfferMsChannelId)}
    };

    protected readonly IHttpConnector _connector;

    public OfferSearchSevices(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<FlightOfferResponse> FlightOfferSearchAsync(FlightOfferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = $"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/search";

        return (await _connector.SendAsync<FlightOfferResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
    }

    public async Task<FlexiDateSearchResponse?> FlexiDateSearchAsync(FlexiDateOfferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FlightSearchRequest == null || request.FlightSearchRequest.CurrentTripIndexId != "0" || request.FlightSearchRequest.SearchOriginDestinations?.Count() != 2)
            return null;

        string url = $"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/FlexiSearch";

        return (await _connector.SendAsync<FlexiDateSearchResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
    }

    public async Task<CalendarSearchResponse?> CalendarSearchAsync(CalendarOfferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FlightSearchRequest?.CurrentTripIndexId != "0")
            return null;

        string url = $"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/CalendarSearch";

        return (await _connector.SendAsync<CalendarSearchResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
    }

    public async Task<OfferFareRules> FareRulesAsync(string offerId)
    {
        ArgumentNullException.ThrowIfNull(offerId);

        string url = $"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/{offerId}/farerules";

        return (await _connector.SendAsync<OfferFareRules>(url, _headers, string.Empty, HttpMethod.Get, null)).Response;
    }
}
