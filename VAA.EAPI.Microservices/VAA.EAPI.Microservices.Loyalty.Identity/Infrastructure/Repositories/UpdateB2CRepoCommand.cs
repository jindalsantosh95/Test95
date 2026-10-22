using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateB2CRepoCommand : IRequest<bool>
    {
        public MemberUpdateRequest MemberUpdateDocument { get; set; }

        public UpdateB2CRepoCommand(MemberUpdateRequest updateDocument)
        {
            MemberUpdateDocument = updateDocument;
        }
    }
}
