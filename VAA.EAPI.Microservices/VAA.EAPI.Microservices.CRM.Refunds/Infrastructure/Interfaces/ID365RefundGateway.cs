using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces
{
    public interface ID365RefundGateway
    {
        Task<string> GetRefundStatusAsync(RefundRequestItem request, CancellationToken cancellationToken);
        Task<string> SubmitRefundAsync(RefundRequestForm request, CancellationToken cancellationToken);
        Task UploadDocument(string claimRef, byte[] document, CancellationToken cancellationToken);
    }
}
