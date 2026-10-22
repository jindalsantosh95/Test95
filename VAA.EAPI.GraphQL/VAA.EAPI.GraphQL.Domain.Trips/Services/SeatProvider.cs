using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class SeatProvider : ISeatProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.SeatMSOcpKey) }, { Common.Constants.Application.ChannelId, Constants.Application.SeatMSDLChannelId } };

        private readonly IHttpConnector _connector;

        public SeatProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<SeatResponse> RetrieveSeatMapAsync(SeatRequest seatMapRequest)
        {
            string retrieveSeatMapUrl = ConfigReader.Get(Common.Constants.Environment.SeatMSUrl);

            return (await _connector.SendAsync<SeatResponse>(retrieveSeatMapUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatMapRequest))).Response;
        }

        public async Task<SeatResponse> RetrieveSeatOfferDetailsAsync(string seatOfferId)
        {
            string retrieveSeatOfferUrl = ConfigReader.Get(Common.Constants.Environment.RetrieveSeatOfferUrl).Replace("{seatOfferId}", seatOfferId);

            return (await _connector.SendAsync<SeatResponse>(retrieveSeatOfferUrl, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<SeatResponse> RetrieveSeatMapWithSelectionAsync(SeatMapSelectionRequest seatMapRequest)
        {
            string retrieveSeatMapWithSelectionUrl = ConfigReader.Get(Common.Constants.Environment.UpdateSeatOfferMSUrl);

            return (await _connector.SendAsync<SeatResponse>(retrieveSeatMapWithSelectionUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatMapRequest))).Response;
        }

        public async Task<SeatDeletionResponse> DeleteSeatAsync(SeatDeletionRequest deleteSeatRequest)
        {
            string seatDeleteUrl = $"{ConfigReader.Get(Common.Constants.Environment.SeatMicroserviceUrl).TrimEnd('/')}/seat/delete";

            return (await _connector.SendAsync<SeatDeletionResponse>(seatDeleteUrl, _headers, string.Empty, HttpMethod.Patch, Json.Serialize(deleteSeatRequest))).Response;
        }

        public async Task<SeatFulfilmentResponse> SeatFulfilmentAsync(SeatFulfilmentRequest seatFulfilmentRequest)
        {
            string seatfulfilmentUrl = $"{ConfigReader.Get(Common.Constants.Environment.SeatMicroserviceUrl).TrimEnd('/')}/SeatFulfilment";            

            return (await _connector.SendAsync<SeatFulfilmentResponse>(seatfulfilmentUrl, _headers, string.Empty, HttpMethod.Patch, Json.Serialize(seatFulfilmentRequest))).Response;
        }
    }
}