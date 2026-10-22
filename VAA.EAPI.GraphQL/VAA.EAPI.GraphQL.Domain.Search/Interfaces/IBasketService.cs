using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IBasketService
    {
        Task<Basket> CreateBasketAsync(Basket Basket);
        Task<Basket> UpdateOfferIDAsync(string offerId, Guid basketId);
        Task<Basket> UpdateOfferIDAsync(string offerId, string basketId);
        Task<Basket> RetrieveBasketAsync(Guid basketId);
        Task<Basket> RetrieveBasketAsync(string basketId);
        Task<Basket> UpdateSelectedOfferAsync(Guid basketId, BookingJourneyOfferDetails selectedOffer);
        Task<Basket> UpdateSelectedOfferAsync(string basketId, BookingJourneyOfferDetails selectedOffer);
        Task<Basket> UpdateFlightSummaryAsync(Guid basketId, string flightSummary, string summaryLink);
        Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink);
        Task<Basket> UpdateLoyaltyMemberDetails(Guid basketId, BasketLoyaltyMemberDetails basketLoyaltyMemberDetails);
        Task<Basket> UpdateLoyaltyMemberDetails(string basketId, BasketLoyaltyMemberDetails basketLoyaltyMemberDetails);
        Task<Basket> UpdateFCEarnPointsAsync(Guid basketId, BasketFCEarnPoints fCEarnPoints);

    }
}
