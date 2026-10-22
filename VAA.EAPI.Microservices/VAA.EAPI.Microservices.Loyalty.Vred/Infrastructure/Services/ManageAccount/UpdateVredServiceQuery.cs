using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.ManageAccount
{
    public class UpdateVredServiceQuery : IRequest<bool>
    {
        public VRedMemberDetails MemberDetailsRequest { get; set; }
        public string Content { get; set; }

        public UpdateVredServiceQuery(VRedMemberDetails memberDetailsRequest, string content)
        {
            MemberDetailsRequest = memberDetailsRequest;
            Content = content;
        }
    }
}
