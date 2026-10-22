using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class DuplicateEmailIdCheckCommand : IRequest<bool>
    {
        public string EmailID { get; set; }

        public DuplicateEmailIdCheckCommand(string emailAddress)
        {
            EmailID = emailAddress;
        }
    }
}
