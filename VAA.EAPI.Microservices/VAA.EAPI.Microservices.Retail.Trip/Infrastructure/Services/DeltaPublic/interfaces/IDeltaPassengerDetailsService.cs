using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces
{
    public interface IDeltaPassengerDetailsService
    {
        Task<PassengerInformationResponse> RetrievePassengerDetails(string tripReservationId);
        Task<PassengerInformationResponse> UpdatePassengerDetails(PassengerInformation passengerInformation);

    }
}
