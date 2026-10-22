using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces
{
    public interface IDeltaMessageDetailsService
    {
        Task<MessageResponse> GetMessageDetailsAsync(string tripBookingID);
    }
}
