using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

public class VerifyMemberCommand : IRequest<Response<string>>
{
    public MemberLoginRequest MemberLoginRequest { get; set; }
}