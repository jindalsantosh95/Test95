using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.OfferMs
{
    public class OfferRetrieveService : IOfferRetrieveService
    {
        private static readonly Dictionary<string, string> _headers = new()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.SearchOfferMsOcpKey)},
            {Common.Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.SearchOfferMsChannelId)}
        };

        protected readonly IHttpConnector _connector;

        public OfferRetrieveService(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<FlightOfferResponse> OfferRetrieveAsync(OfferSearchRequest searchRequest)
        {
            if (searchRequest is null) throw new ArgumentNullException(nameof(searchRequest));

            return (await _connector.SendAsync<FlightOfferResponse>($"{ConfigReader.Get(Constants.Environment.SearchOfferMsUrl)}/{searchRequest.OfferID}", _headers, string.Empty, HttpMethod.Get, Json.Serialize(searchRequest))).Response;
        }
    }
}
