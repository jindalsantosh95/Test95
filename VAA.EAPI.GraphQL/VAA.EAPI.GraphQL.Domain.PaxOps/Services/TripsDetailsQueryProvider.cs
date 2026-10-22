using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;
using TripMS = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;


namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class TripsDetailsQueryProvider : ITripsDetailsQueryProvider
    {
        private static readonly Dictionary<string, string> _headers =
            new() { { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.AccountTripMsOcpKey) } };

        protected readonly IHttpConnector _connector;

        public TripsDetailsQueryProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<TripMS.Booking> GetTripDetails(TripMS.TripsDetailsRequest tripDetailsRequest)
        {
            ArgumentNullException.ThrowIfNull(tripDetailsRequest);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/trips/search";

            var resp = (await _connector.SendAsync<TripMS.TripsDetailsResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(tripDetailsRequest))).Response.Booking; ;
            return resp;
        }

        public async Task<PassengerDetailsResponse> GetPassengerDetails(string bookingId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(bookingId);
            var encodedBookingId = Uri.EscapeDataString(bookingId);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/passengerDetails/{encodedBookingId}";

            return (await _connector.GetAsync<PassengerDetailsResponse>(url, _headers, string.Empty)).Response;
        }

        public async Task<PassengerDetailsResponse> UpdatePassengerDetails(UpdatePassengerDetailsRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var encodedBookingId = Uri.EscapeDataString(request.BookingId);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/passengerDetails/{encodedBookingId}";
            var payload = (PassengerDetailsResponse)request;
            return (await _connector.SendAsync<PassengerDetailsResponse>(url, _headers, string.Empty, HttpMethod.Patch, Json.Serialize(payload))).Response;
        }
    }
}
