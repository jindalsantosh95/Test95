using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;


namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class PassengerDetailsProvider : IPassengerDetailsProvider
    {
        private static readonly Dictionary<string, string> _headers =
            new()
            {
                    {
                        CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.AccountTripMsOcpKey)
                    }
            };

        protected readonly IHttpConnector _connector;

        public PassengerDetailsProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PassengerDetails> GetPassengerDetails(string tripReservationId)
        {
            ArgumentNullException.ThrowIfNull(tripReservationId);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl).TrimEnd('/')}/passengerDetails/{tripReservationId}";

            return (await _connector.GetAsync<PassengerDetails>(url, _headers, string.Empty)).Response;
        }

        public async Task<PassengerDetails> ManagePassengerLoyaltyMemberDetails(PassengerDetails passengerDetails)
        {
            ArgumentNullException.ThrowIfNull(passengerDetails);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl).TrimEnd('/')}/passengerDetails/{passengerDetails.PassengerInformation?.bookingId}";

            return (await _connector.SendAsync<PassengerDetails>(url, _headers, string.Empty, HttpMethod.Patch, Json.Serialize(passengerDetails))).Response;
        }
    }
}

