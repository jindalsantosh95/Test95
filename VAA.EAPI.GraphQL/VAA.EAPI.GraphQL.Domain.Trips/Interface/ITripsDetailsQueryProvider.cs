using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.DeepLink;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using TripMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ITripsDetailsQueryProvider
    {
        Task<TripMS.Booking> GetTripDetails(TripMS.TripsDetailsRequest tripDetailsRequest);
        Task<MessageResponse> GetTripMessageDetails(string tripReservationID);
        Task<IEnumerable<FlyReadyStatus>> GetFlyreadyTripMessageStatus(string tripReservationID);
        Task<DeepLinkDetail> GenerateDeeplinkForProxy(string id);
        Task<DeeplinkResponse> GetDeeplinkUrl(DeeplinkRequest request);
        Task<TripMS.TripListResponseStore> GetTripDetailsList(IEnumerable<TripMS.TripsDetailsRequest> tripDetailsRequest);
    }
}