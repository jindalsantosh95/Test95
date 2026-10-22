using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class UpdateVredRepoQuery : IRequest<bool>
    {
        public UpdateVredMemberDetailsRequest UpdateVredMemberDetails { get; set; }

        public UpdateVredRepoQuery(UpdateVredMemberDetailsRequest updateVredMemberDetails)
        {
            UpdateVredMemberDetails = updateVredMemberDetails;
        }
    }
}
