using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class BasketProvider : IBasketProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.BasketMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public BasketProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Basket> CreateBasketAsync(CreateBasketRequest basket)
        {
            string createBasketUri = ConfigReader.Get(Common.Constants.Environment.BasketMSUrl).TrimEnd('/');

            return (await _connector.SendAsync<Basket>(createBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(basket))).Response;
        }

        public async Task<Basket> RetrieveBasketAsync(string basketId)
        {
            string retrieveBasketUri = $"{ConfigReader.Get(Common.Constants.Environment.BasketMSUrl)}{basketId}";

            return (await _connector.GetAsync<Basket>(retrieveBasketUri, _headers, string.Empty)).Response;
        }

        public async Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest)
        {
            string updateBasketSeatOfferUrl = $"{ConfigReader.Get(Common.Constants.Environment.BasketMSUrl)}{seatUpdateRequest.BasketId}/seat";

            return (await _connector.SendAsync<Basket>(updateBasketSeatOfferUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatUpdateRequest.FlightPassengerSeatMap))).Response;
        }

        public async Task<bool> DeleteBasketAsync(string basketid)
        {
            string deleteBasketUri = $"{ConfigReader.Get(Common.Constants.Environment.BasketMSUrl)}{basketid}";

            return (await _connector.DeleteAsync<bool>(deleteBasketUri, _headers, string.Empty)).Response;
        }

        public async Task<Basket> UpdatePaymentDetailsAsync(UpdateBasketPaymentRequest updateBasketPayment)
        {
            string updateBasketUri = $"{ConfigReader.Get(Common.Constants.Environment.BasketMSUrl)}{updateBasketPayment.BasketId}/payment";

            return (await _connector.SendAsync<Basket>(updateBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(updateBasketPayment.PaymentSessionsBasketMS))).Response;
        }

        public async Task<Basket> UpdateBasketOrderStatusAsync(string basketId, string status)
        {
            string processBasketUri = $"{ConfigReader.Try(Common.Constants.Environment.BasketMSUrl)}{basketId}/order";

            return (await _connector.SendAsync<Basket>(processBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new { Status = status }))).Response;
        }

        public async Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetail)
        {          
            string updateContactBasketUri = $"{ConfigReader.Get(Common.Constants.Environment.BasketMSUrl)}{contactDetail.BasketId}/contact";

            return (await _connector.SendAsync<Basket>(updateContactBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(contactDetail))).Response;
        }
    }
}