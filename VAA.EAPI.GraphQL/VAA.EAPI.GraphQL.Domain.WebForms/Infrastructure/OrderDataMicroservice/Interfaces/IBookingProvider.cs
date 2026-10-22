using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Interfaces
{
    public interface IBookingProvider
    {
        Task<RefundForm> FindMyBooking(RefundForm requestForm);
    }
}
