using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IOrderDataProvider
    {
        Task<FlightBookingsResponse> RetrieveBookings(string fcNumber, string emailId);
    }
}
