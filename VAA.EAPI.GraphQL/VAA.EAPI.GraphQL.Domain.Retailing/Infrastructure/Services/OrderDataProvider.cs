using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services
{
    public sealed class OrderDataProvider(IHttpConnector httpConnector) : IOrderDataProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<FlightBookingsResponse> RetrieveBookings(string fcNumber)
        {

            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("OrderDataProvider:OcpKey") },
                { "FCNumber", fcNumber } ,
                {"Email", string.Empty } ,
                {"PhoneNo", string.Empty }
            };

            return (await _httpConnector.GetAsync<FlightBookingsResponse>(GetResourceUrl("/futurebookings"), headers, string.Empty)).Response;            
        }

        public async Task<string> UpdateSSRs(OrderMutationRequest request)
        {
            // ADVISORY: This mutation should verify the list of SSRs that are allowed to be mutated to ensure that random SSRs are not added / removed to the booking.
            if (request.ShipmentRequestId == "FAILME") throw new Exception("Reservation system temporarily unavailable.");

            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("OrderDataProvider:OcpKey") } };

            var response = await _httpConnector.SendAsync(GetResourceUrl("/ssrs"), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private static string GetResourceUrl(string resource) => $"{ConfigReader.Get("OrderDataProvider:MsUrl").Trim().TrimEnd('/')}/{resource?.Trim().TrimStart('/')}";
    }
}
