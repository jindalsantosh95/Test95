using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Interfaces
{
    public interface IRefundProvider
    {
        Task<RefundForm> CheckForDuplicates(RefundForm form);

        Task<string> SubmitRefundRequest(RefundForm form);
    }
}
