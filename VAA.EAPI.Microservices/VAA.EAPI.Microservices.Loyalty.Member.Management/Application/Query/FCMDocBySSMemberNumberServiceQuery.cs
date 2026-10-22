using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class FCMDocBySSMemberNumberServiceQuery : IRequest<Result<Response<FlyingClubMember>>>
    {
        public string SSAMemberNumber { get; set; }
    }
}
