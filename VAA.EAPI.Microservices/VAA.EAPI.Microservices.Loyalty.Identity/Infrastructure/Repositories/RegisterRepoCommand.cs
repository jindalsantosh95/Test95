using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class RegisterRepoCommand : IRequest<B2CUserResponse>
    {
        public MemberRegisterRequest MemberRegisterRequest { get; set; }

        public bool AccountEnabled { get; }

        public RegisterRepoCommand(MemberRegisterRequest memberRegisterRequest, bool accountEnabled = true)
        {
            MemberRegisterRequest = memberRegisterRequest;
            AccountEnabled = accountEnabled;
        }
    }
}
