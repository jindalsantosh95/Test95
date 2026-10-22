using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IDeltaMessageDetailsPrivateService
    {
        Task<MessageResponse> GetMessageDetailsAsync(string tripBookingID);
    }
}
