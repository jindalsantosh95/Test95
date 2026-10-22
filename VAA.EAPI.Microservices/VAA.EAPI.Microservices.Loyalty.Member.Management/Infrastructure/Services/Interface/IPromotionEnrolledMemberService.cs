using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface IPromotionEnrolledMemberService
    {
        public Task<Result<Response<string>>> CreateXlStatementAndUploadToBlob(IEnumerable<PromotionEnrolledMembers> memberList);
    }
}
