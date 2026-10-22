using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class SeatServiceProvider : ISeatServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BookSeatMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public SeatServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<SeatMSResponse> RetrieveSeatMapAsync(SeatMSRequest seatMapRequest)
        {
            string retrieveSeatMapUrl = ConfigReader.Get(Constants.Environment.BookSeatMSUrl);

            return (await _connector.SendAsync<SeatMSResponse>(retrieveSeatMapUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatMapRequest))).Response;
        }

        public async Task<SeatMSResponse> RetrieveSeatOfferDetailsAsync(string offerId)
        {
            string retrieveSeatOfferUrl = ConfigReader.Get(Constants.Environment.BookRetrieveSeatOfferUrl).Replace("{seatOfferId}", offerId);

            return (await _connector.SendAsync<SeatMSResponse>(retrieveSeatOfferUrl, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<SeatMSResponse> RetrieveSeatMapWithSelectionAsync(SeatMapSelectionMSRequest seatMapRequest)
        {
            string retrieveSeatMapUrl = ConfigReader.Get(Constants.Environment.BookUpdateSeatOfferMSUrl);

            return (await _connector.SendAsync<SeatMSResponse>(retrieveSeatMapUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatMapRequest))).Response;
        }
    }
}