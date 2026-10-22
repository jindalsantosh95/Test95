using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class BasketServiceProvider : IBasketServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.BasketMSOcpKey) } };

        private static readonly string _basketMsUrl = ConfigReader.Get(Common.Constants.Environment.BasketMSUrl);

        private readonly IHttpConnector _connector;

        public BasketServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ)
        {
            return (await _connector.SendAsync<Basket>(_basketMsUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(createBasketRQ))).Response;
        }

        public async Task<Basket> RetrieveBasketAsync(string basketId)
        {
            string retireveBasketUri = $"{_basketMsUrl}{basketId}";

            return (await _connector.SendAsync<Basket>(retireveBasketUri, _headers, string.Empty, HttpMethod.Get, null)).Response;
        }

        public async Task<Basket> UpdatePaymentDetailsAsync(UpdateBasketPaymentRequest updateBasketPayment)
        {
            string updateBasketUri = $"{_basketMsUrl}{updateBasketPayment.BasketId}/payment";

            return (await _connector.SendAsync<Basket>(updateBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(updateBasketPayment.PaymentSessionsBasketMS))).Response;
        }

        public async Task<Basket> UpdateBasketOrderStatusAsync(string basketId, string status)
        {
            string processBasketUri = $"{_basketMsUrl}{basketId}/order";

            return (await _connector.SendAsync<Basket>(processBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new { Status = status }))).Response;
        }
    }
}
