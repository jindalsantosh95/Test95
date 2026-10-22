using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class OrderDataServiceProvider : IOrderDataServiceProvider
    {
        private static readonly Dictionary<string, string> _headers =
            new()
            { {  CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.OrderDataOcpKey) } };

        protected readonly IHttpConnector _connector;

        public OrderDataServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<Booking>> GetTripDetails(Infrastructure.Models.OrderDataMS.TripDetailsRequest request)
        {
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.OrderDataMSUrl)}/trips/details";

            return (await _connector.SendAsync<IEnumerable<Booking>>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }
    }
}