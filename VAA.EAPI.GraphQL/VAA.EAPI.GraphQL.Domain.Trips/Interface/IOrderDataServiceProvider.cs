using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IOrderDataServiceProvider
    {
        Task<IEnumerable<Booking>> GetTripDetails(Infrastructure.Models.OrderDataMS.TripDetailsRequest tripDetailsRequest);
    }
}