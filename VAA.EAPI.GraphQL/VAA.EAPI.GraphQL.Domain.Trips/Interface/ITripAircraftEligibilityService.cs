using TripMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface;

public interface ITripAircraftEligibilityService
{
    Task EnrichAircraftDetailsAsync(IEnumerable<TripMS.Booking> bookings, TripAircraftEnrichmentOptions options);
}
