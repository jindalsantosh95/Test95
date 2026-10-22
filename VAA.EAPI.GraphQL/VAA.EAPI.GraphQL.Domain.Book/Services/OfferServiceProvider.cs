using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public sealed class OfferServiceProvider : IOfferServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BookOffersSearchOcpKey)},
            {Common.Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.BookOfferMsChannelId)}
        };

        protected readonly IHttpConnector _connector;

        public OfferServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<OfferResponse> OfferRetrieveAsync(string offerId)
        {
            string offerRetrieveUri = ConfigReader.Get(Constants.Environment.BookOffersSearchUrl).Replace("{offerId}", offerId);

            return (await _connector.GetAsync<OfferResponse>(offerRetrieveUri, _headers, string.Empty)).Response;
        }
    }
}
