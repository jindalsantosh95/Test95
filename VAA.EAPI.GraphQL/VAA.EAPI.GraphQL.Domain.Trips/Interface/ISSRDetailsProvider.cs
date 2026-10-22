using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ISSRDetailsProvider
    {
        Task<PassengerSSRResponse> QuerySSRMealDetailsAsync(string tripResevationID);
        Task<PassengerSSRResponse> ManageSSRAsync(SSRUpsertRequest ssrUpsertRequest, string tripReservationId);
    }
}