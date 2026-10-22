using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IDeltaEncryptionPrivateService
    {
        Task<EncryptionResponse> GetEncryptedBookingDetailsAsync(string tripBookingId);
    }
}