using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount
{
    public class UpdateVRedMemberResponse : IRequest<VRedMemberDetails>
    {
        public VRedErrorResponse ErrorResponse { get; set; }
    }
}
