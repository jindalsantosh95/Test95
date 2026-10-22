using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services
{
    public sealed class OrderSearchProvider(IHttpConnector httpConnector) : IOrderSearchProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<OrderSearchResponse> RetrieveBooking(string bookingRef, string firstName, string lastName)
        {            
            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("OrderSearchProvider:OcpKey") },
                { Common.Constants.Application.ChannelId,ConfigReader.Get("Retailing:ChannelId") },               
            };

            return (await _httpConnector.SendAsync<OrderSearchResponse>(GetResourceUrl("/order/Search"), headers, string.Empty, HttpMethod.Post, Json.Serialize(OrderSearchMapper.RequestMapper(bookingRef, firstName, lastName)))).Response;            
        }

        private static string GetResourceUrl(string resource) => $"{ConfigReader.Get("OrderSearchProvider:OrderSearchMsUri").Trim().TrimEnd('/')}/{resource?.Trim().TrimStart('/')}";
    }
}
