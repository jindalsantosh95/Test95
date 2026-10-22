using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IOrderMutationProvider
    {
        Task<OrderResponseMS> CreateOrder(OrderCreateRequestMS request);
    }
}
