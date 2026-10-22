using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories
{
    public interface IRemoveDuplicatePnrRepoCommand
    {
        Task<Result<Response<RemoveDuplicatePnrResponse>>> Handle(RemoveDuplicatePnrRequest request, CancellationToken cancellationToken);
    }
}