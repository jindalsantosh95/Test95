using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IBasketEarnPointsCalculatorService
    {
        Task<FCEarnPoints> Calculate(Basket basket, OfferResponse offer, CustomerDTO leadCustomer = null);
    }
}
