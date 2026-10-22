using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class VerifyMemberRepoCommand : IRequest<bool>
    {
        public MemberLoginRequest MemberLoginRequest { get; set; }

        public VerifyMemberRepoCommand(MemberLoginRequest memberLoginRequest)
        {
            MemberLoginRequest = memberLoginRequest;
        }
    }
}
