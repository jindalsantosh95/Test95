using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface
{
    public interface ICachedMemberAccountReferenceData
    {
        Task<Result<Response<MemberAccountReferenceData>>> GetCachedMemberAccountReferenceData();
    }
}
