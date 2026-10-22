using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.CheckInMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ITravelRequirementsQueryProvider
    {
        Task<TravelRequirementsResponse> GetTravelRequirements(TravelRequirementsRequest travelRequirementsRequest);
    }
}
