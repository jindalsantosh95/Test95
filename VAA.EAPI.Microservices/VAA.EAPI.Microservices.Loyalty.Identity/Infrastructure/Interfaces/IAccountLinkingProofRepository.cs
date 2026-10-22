using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IAccountLinkingProofRepository
    {
        Task<DateTime> StoreBoundAsync(string proofHash, CreateLinkingProofRequest request, string consentReference, TimeSpan lifetime, CancellationToken cancellationToken);
        Task<RedeemLinkingProofResponse> ConsumeBoundAsync(string proofHash, string transactionId, string transactionBindingHash, CancellationToken cancellationToken);
    }
}
