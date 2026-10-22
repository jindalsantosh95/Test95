using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.OrderEmail;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface
{
    public interface IEmailService
    {
        Task AddEventForEmailReceiptAsync(BagsOrderEmailRequest request);
    }
}