using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces
{
    public interface IDeltaTripsDetailsService
    {
        Task<DeltaTripsDetailsResponse> RetrieveTripsDetails(DeltaTripsDetailsRequest deltaTripsDetailsRequest);

    }
}
