using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IBasketProvider
    {
        Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ);
        Task<Basket> RetrieveBasketAsync(string basketId);
        Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest);
        Task<Basket> UpdatePaymentDetailsAsync(UpdateBasketPaymentRequest updateBasketPayment);
        Task<bool> DeleteBasketAsync(string basketId);
        Task<Basket> UpdateBasketOrderStatusAsync(string basketid, string status);
        Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetail);
    }
}