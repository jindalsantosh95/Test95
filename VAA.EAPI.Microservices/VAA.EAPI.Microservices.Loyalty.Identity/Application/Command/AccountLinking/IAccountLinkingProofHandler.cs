using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountLinking
{
    public interface IAccountLinkingProofHandler
    {
        Task<Result<Response<CreateLinkingProofResponse>>> CreateAsync(CreateLinkingProofRequest request, CancellationToken cancellationToken);
        Task<Result<Response<RedeemLinkingProofResponse>>> RedeemAsync(RedeemLinkingProofRequest request, CancellationToken cancellationToken);
    }
}
