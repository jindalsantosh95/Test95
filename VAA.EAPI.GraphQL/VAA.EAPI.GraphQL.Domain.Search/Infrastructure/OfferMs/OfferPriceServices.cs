using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.OfferMs;

public class OfferPriceServices : IOfferPriceServices
{
    private static readonly Dictionary<string, string> _headers = new()
    {
        {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.SearchOfferMsOcpKey)},
        {Common.Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.SearchOfferMsChannelId)}
    };

    protected readonly IHttpConnector _connector;

    public OfferPriceServices(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<FlightOfferResponse> OfferPriceAsync(FlightOfferPriceRequest flightOfferPriceRequest)
    {
        if (flightOfferPriceRequest is null) throw new ArgumentNullException(nameof(flightOfferPriceRequest));

        return (await _connector.SendAsync<FlightOfferResponse>($"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/search/price", _headers, string.Empty, HttpMethod.Post, Json.Serialize(flightOfferPriceRequest))).Response;
    }

    public async Task<FlightOfferResponse> OfferPriceDeeplinkAsync(FlightDeepLinkSummaryRequest deepLinkSummaryRequest)
    {
        if (deepLinkSummaryRequest is null) throw new ArgumentNullException(nameof(deepLinkSummaryRequest));

        return (await _connector.SendAsync<FlightOfferResponse>($"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/search/OfferPriceDeepLink", _headers, string.Empty, HttpMethod.Post, Json.Serialize(deepLinkSummaryRequest))).Response;
    }
}
