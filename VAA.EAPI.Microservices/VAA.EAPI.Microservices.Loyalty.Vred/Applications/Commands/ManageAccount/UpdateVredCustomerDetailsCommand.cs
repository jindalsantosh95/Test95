using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount
{
    public class UpdateVredCustomerDetailsCommand : CloudEvent
    {
        public UpdateVredMemberDetailsRequest Request { get; set; }
    }
}
