using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Interfaces
{
    public interface IRefundRequestService
    {
        Task<RefundForm> FindMyBooking(RefundForm form);
        Task<string> SubmitRefundRequest(RefundForm form);
    }
}
