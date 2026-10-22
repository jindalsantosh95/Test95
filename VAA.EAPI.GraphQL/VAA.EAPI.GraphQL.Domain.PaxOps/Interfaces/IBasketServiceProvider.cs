using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface IBasketServiceProvider
    {
        Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ);
        Task<Basket> RetrieveBasketAsync(string basketId);
        Task<Basket> UpdatePaymentDetailsAsync(UpdateBasketPaymentRequest updateBasketPayment);
        Task<Basket> UpdateBasketOrderStatusAsync(string basketId, string status);

    }
}

