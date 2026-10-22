using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface ITripsDetailsQueryProvider
    {
        Task<Booking> GetTripDetails(TripsDetailsRequest tripDetailsRequest);
        Task<PassengerDetailsResponse> GetPassengerDetails(string bookingId);
        Task<PassengerDetailsResponse> UpdatePassengerDetails(UpdatePassengerDetailsRequest request);
    }
}
