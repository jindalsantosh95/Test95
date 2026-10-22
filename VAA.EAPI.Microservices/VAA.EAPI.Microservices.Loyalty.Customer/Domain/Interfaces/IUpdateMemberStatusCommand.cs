using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces
{
    public interface IUpdateMemberStatusCommand
    {
        Task<Result<Response<string>>> Handle(BulkMemberStatusUpdateRequest bulkMemberStatusUpdateRequest, CancellationToken cancellationToken);
    }
}