using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;
namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class UpsellOfferDetailsProvider : IUpsellOfferDetailsProvider
    {
        private static readonly Dictionary<string, string> _headers =
           new()
           {
               { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.AccountTripMsOcpKey) }
           };

        protected readonly IHttpConnector _connector;

        public UpsellOfferDetailsProvider(IHttpConnector connector) => _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        public async Task<UpsellOfferResponse> QueryUpsellOfferDetails(string tripResevationId)
        {
            ArgumentNullException.ThrowIfNull(tripResevationId);

            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/upselloffers/{tripResevationId}";
            return (await _connector.GetAsync<UpsellOfferResponse>(url, _headers, string.Empty))?.Response;
        }
    }
}