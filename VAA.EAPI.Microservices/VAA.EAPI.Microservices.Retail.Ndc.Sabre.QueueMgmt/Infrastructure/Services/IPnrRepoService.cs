using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Services
{
    public interface IPnrRepoService
    {
        Task<Result<Response<DuplicatePnrResponse>>> FetchDuplicatePnrs(string queue, string pcc, CancellationToken cancellationToken);
        Task<Result<Response<RemoveDuplicatePnrResponse>>> RemoveDuplicate(string pcc, string pnr, CancellationToken cancellationToken);
    }
}