using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IDeltaPassengerDetailsPrivateService
    {
        Task<PassengerInformationResponse> RetrievePassengerDetails(string tripReservationId);
        Task<PassengerInformationResponse> UpdatePassengerDetails(PassengerInformation passengerInformation);

    }
}
