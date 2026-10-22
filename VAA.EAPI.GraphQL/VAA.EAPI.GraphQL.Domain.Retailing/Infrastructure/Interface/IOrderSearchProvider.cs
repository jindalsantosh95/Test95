using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface
{
    public interface IOrderSearchProvider
    {
        Task<OrderSearchResponse> RetrieveBooking(string bookingRef, string firstName, string lastName);
    }
}
