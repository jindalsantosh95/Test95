using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UnVerifiedMemberRepoCommand : IRequest<string>
    {
        public MemberLoginRequest MemberLoginRequest { get; set; }

        public UnVerifiedMemberRepoCommand(MemberLoginRequest memberLoginRequest)
        {
            MemberLoginRequest = memberLoginRequest;
        }
    }

}
