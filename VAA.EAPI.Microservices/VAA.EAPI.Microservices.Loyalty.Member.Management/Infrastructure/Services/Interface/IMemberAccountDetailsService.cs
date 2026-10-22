using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface IMemberAccountDetailsService
    {
        public Task<Result<Response<MemberAccountResponse>>> GetMemberAccountDetails(MemberAccountRequestQuery request);
    }
}
