using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class PromotionEnrolledMemberDetailsQuery : IRequest<Result<Response<PromotionEnrolledMemberDetailsResponse>>>
    {
        public string PromotionId { get; set; }
    }
}
