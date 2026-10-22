using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories
{
    public interface IDuplicatePnrRepoCommand
    {
        Task<Result<Response<DuplicatePnrResponse>>> Handle(DuplicatePnrRequest request, CancellationToken cancellationToken);
    }
}
