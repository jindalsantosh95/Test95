using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IDeltaTripsDetailsPrivateService
    {
        Task<DeltaTripsDetailsResponse> RetrieveTripsDetails(DeltaTripsDetailsRequest deltaTripsDetailsRequest);

    }
}
