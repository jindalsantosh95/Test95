using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface
{
    public interface IOrderDataProvider
    {
        Task<FlightBookingsResponse> RetrieveBookings(string fcNumber);
        Task<string> UpdateSSRs(OrderMutationRequest request);
    }
}
