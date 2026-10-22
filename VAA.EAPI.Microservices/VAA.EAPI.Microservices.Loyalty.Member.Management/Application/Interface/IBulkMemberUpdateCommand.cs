using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IBulkMemberUpdateCommand
    {
        Task<Result<Response<BulkMemberUpdateResponses>>> Handle(BulkMemberUpdateRequest bulkMemberUpdateRequest, CancellationToken cancellationToken);
    }
}
