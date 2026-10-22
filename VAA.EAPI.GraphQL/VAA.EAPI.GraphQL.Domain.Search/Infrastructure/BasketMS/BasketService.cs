using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BasketMS
{
    public class BasketService : IBasketService
    {
        protected readonly IHttpConnector _connector;
        protected readonly ILogger<BasketService> _logger;
        private static Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BasketMSOcpKey) } };

        public BasketService(ILogger<BasketService> logger, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Basket> UpdateOfferIDAsync(string offerID, Guid basketId) => await UpdateOfferIDAsync(offerID, basketId.ToString());
        public async Task<Basket> UpdateOfferIDAsync(string offerID, string basketId)
        {
            if (offerID is null)
                throw new ArgumentNullException(nameof(offerID));

            return (await _connector.SendAsync<Basket>(ConfigReader.Get(Constants.Environment.BasketMSUpdateOfferIDUrl).Replace("{basket}", basketId).Replace("{offerId}", offerID), headers, string.Empty, HttpMethod.Post, Json.Serialize(new()))).Response;
        }

        public async Task<Basket> UpdateSelectedOfferAsync(Guid basketId, BookingJourneyOfferDetails selectedOffer) => await UpdateSelectedOfferAsync(basketId.ToString(), selectedOffer);
        public async Task<Basket> UpdateSelectedOfferAsync(string basketId, BookingJourneyOfferDetails selectedOffer)
        {
            if (selectedOffer is null)
                throw new ArgumentNullException(nameof(selectedOffer));

            return (await _connector.SendAsync<Basket>(ConfigReader.Get(Constants.Environment.BasketMSUpdateSelectedOfferUrl).Replace("{basket}", basketId), headers, string.Empty, HttpMethod.Post, Json.Serialize(selectedOffer))).Response;
        }

        public async Task<Basket> CreateBasketAsync(Basket basket)
        {
            if (basket is null)
                throw new ArgumentNullException(nameof(basket));

            return (await _connector.SendAsync<Basket>(ConfigReader.Get(Constants.Environment.BasketMSCreateUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(basket))).Response;
        }

        public async Task<Basket> RetrieveBasketAsync(Guid basketId) => await RetrieveBasketAsync(basketId.ToString());
        public async Task<Basket> RetrieveBasketAsync(string basketId)
        {
            return (await _connector.GetAsync<Basket>(ConfigReader.Get(Constants.Environment.BasketMSRetrieveUrl).Replace("{basket}", basketId), headers, string.Empty)).Response;
        }

        public async Task<Basket> UpdateFlightSummaryAsync(Guid basketId, string flightSummary, string summaryLink) => await UpdateFlightSummaryAsync(basketId.ToString(), flightSummary, summaryLink);
        public async Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink)
        {
            if (flightSummary.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(flightSummary));

            return (await _connector.SendAsync<Basket>(ConfigReader.Get(Constants.Environment.BasketMSSetFlightSummaryUrl).Replace("{basket}", basketId), headers, string.Empty, HttpMethod.Post, Json.Serialize(new { FlightSummary = flightSummary, SummaryLink = summaryLink }))).Response;
        }

        public async Task<Basket> UpdateLoyaltyMemberDetails(Guid basketId, BasketLoyaltyMemberDetails basketLoyaltyMemberDetails) => await UpdateLoyaltyMemberDetails(basketId.ToString(), basketLoyaltyMemberDetails);
        public async Task<Basket> UpdateLoyaltyMemberDetails(string basketId, BasketLoyaltyMemberDetails basketLoyaltyMemberDetails)
        {
            if (basketLoyaltyMemberDetails == null)
                throw new ArgumentNullException(nameof(basketLoyaltyMemberDetails));

            string updateMembershipDetailsUrl = ConfigReader.Try<string>(Constants.Environment.BasketMSUpdateMembershipUrl).Replace("{basket}", basketId);

            return (await _connector.SendAsync<Basket>(updateMembershipDetailsUrl, headers, string.Empty, HttpMethod.Post, Json.Serialize(basketLoyaltyMemberDetails))).Response;
        }

        public async Task<Basket> UpdateFCEarnPointsAsync(Guid basketId, BasketFCEarnPoints fCEarnPoints)
        {
            string updateFCEarnPointsUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}{Common.Constants.Application.UpdateFCEarnPointsEndPoint}";

            return (await _connector.SendAsync<Basket>(updateFCEarnPointsUrl, headers, string.Empty, HttpMethod.Post, Json.Serialize(fCEarnPoints))).Response;
        }
    }
}
