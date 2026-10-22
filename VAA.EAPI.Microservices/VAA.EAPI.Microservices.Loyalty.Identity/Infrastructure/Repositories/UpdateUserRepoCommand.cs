using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateUserRepoCommand : IRequest<B2CUserResponse>
    {
        public string ObjectID { get; set; }
        public UpdateUserRepoCommand(string objectID)
        {
            ObjectID = objectID;
        }
    }
}
