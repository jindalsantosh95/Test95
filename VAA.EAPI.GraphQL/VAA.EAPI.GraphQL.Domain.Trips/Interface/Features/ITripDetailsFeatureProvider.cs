using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface.Features
{
    public interface ITripDetailsFeatureProvider
    {
        Task<Booking> AddTripDetailFeature(Infrastructure.Models.TripMS.Booking bookingDetails, IEnumerable<CabinRefData> cabinDetails, string bookingId, IEnumerable<AirportRefData> airportRefDatas, IEnumerable<AirlineRefData> airlineRefDatas, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements = null);
    }
}
