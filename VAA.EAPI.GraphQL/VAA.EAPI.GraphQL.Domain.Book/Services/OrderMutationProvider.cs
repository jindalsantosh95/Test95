using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class OrderMutationProvider : IOrderMutationProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BookOrderOcpKey) } };

        private readonly IHttpConnector _connector;

        public OrderMutationProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<OrderResponseMS> CreateOrder(OrderCreateRequestMS createOrder)
        {
            string createOrderUri = ConfigReader.Get(Constants.Environment.BookOrderCreateMSUrl);

            return (await _connector.SendAsync<OrderResponseMS>(createOrderUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(createOrder))).Response;

        }
    }
}
