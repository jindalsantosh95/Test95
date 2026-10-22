using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;
namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class SSRDetailsProvider : ISSRDetailsProvider
    {
        private static readonly Dictionary<string, string> _headers =
           new()
           { { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.FlightServiceMsOcpKey)}};

        protected readonly IHttpConnector _connector;

        public SSRDetailsProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PassengerSSRResponse> QuerySSRMealDetailsAsync(string tripBookingID)
        {
            ArgumentNullException.ThrowIfNull(tripBookingID);

            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.FlightServiceMsUrl)}/ssrs/{tripBookingID}";
            return (await _connector.GetAsync<PassengerSSRResponse>(url, _headers, string.Empty))?.Response;
        }

        public async Task<PassengerSSRResponse> ManageSSRAsync(SSRUpsertRequest ssrUpsertRequest, string tripbookingId)
        {
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.FlightServiceMsUrl)}/ssrs/{tripbookingId}";
            var response = (await _connector.SendAsync<PassengerSSRResponse>(url, _headers, string.Empty, HttpMethod.Patch, Json.Serialize(ssrUpsertRequest))).Response;

            return new PassengerSSRResponse()
            {
                Passengers = response.Passengers.Where(p => p.PassengerResourceId == ssrUpsertRequest.PassengerResourceId)
            };
        }
    }
}