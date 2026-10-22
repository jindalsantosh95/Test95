using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IBulkMemberUpdateMutationProvider
    {
        Task<BulkMemberUpdateResponse> BulkUpdateWithFile(BulkMemberUpdateRequest request);
    }
}