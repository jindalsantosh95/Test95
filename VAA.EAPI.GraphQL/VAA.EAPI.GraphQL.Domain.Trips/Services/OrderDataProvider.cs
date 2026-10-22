using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public sealed class OrderDataProvider(IHttpConnector httpConnector) : IOrderDataProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<FlightBookingsResponse> RetrieveBookings(string fcNumber, string emailId)
        {

            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("OrderDataProvider:OcpKey") },
                { "FCNumber", fcNumber } ,
                {"Email", emailId } ,
                {"PhoneNo", string.Empty }
            };

            return (await _httpConnector.GetAsync<FlightBookingsResponse>(GetResourceUrl("/futurebookings"), headers, string.Empty)).Response;
        }

        private static string GetResourceUrl(string resource) => $"{ConfigReader.Get("OrderDataProvider:MsUrl").Trim().TrimEnd('/')}/{resource?.Trim().TrimStart('/')}";

    }
}
