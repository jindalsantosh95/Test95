using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces
{
    public interface IDeltaEncryptionService
    {
        Task<EncryptionResponse> GetEncryptedBookingDetailsAsync(string tripBookingId);
    }
}