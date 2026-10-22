using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IBasketServiceProvider
    {
        Task<Basket> RetrieveBasketAsync(string offerId);
        Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ);
        Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetails);
        Task<Basket> UpdatePeopleAsync(PassengerUpdateRequest people);
        Task<Basket> UpdateTnCAsync(TnCUpdateRequest termsAndConditions);
        Task<Basket> UpdatePaymentDetailsAsync(BasketPaymentRequest basketPaymentRequest);
        Task<bool> DeleteBasketAsync(string basketId);
        Task<Basket> UpdateBasketOrderStatusAsync(string basketid, string status);
        Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest);
        Task<Basket> UpdateCustomerProfileAsync(string basketId, BasketLoyaltyMemberDetails loyaltyMemberDetails);
        Task<Basket> UpdateFCEarnPointsAsync(string basketId, FCEarnPoints fCEarnPoints);
        Task UpdateVoucherRedemptionAsync(string basketId, VoucherRedemptionUpdateRequest request);
        Task<Basket> UpdatePointsCurrencyDetailsAsync(string basketId, IEnumerable<PtcWisePointsEquivalentCurrencyDetails> pointsCurrencyDetails);
        Task<Basket> SaveInsuranceQuoteAsync(string basketId, IEnumerable<QuoteResponse> quoteResponse);
        Task<Basket> UpdateInsuranceDecisionAsync(string basketId, bool insuranceAdded);
        Task<Basket> SetCobrandProgress(string basketId, string cobrandProgress);
        Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink);
    }
}
