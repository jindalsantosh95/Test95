using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount
{
    public class UpdateVredMemberDetailsRequestDataPayload : CloudEvent
    {
        public UpdateVredMemberDetailsRequest Data
        {
            get => base.Data as UpdateVredMemberDetailsRequest;
            set => base.Data = value;
        }
    }
    public class UpdateVredMemberDetailsRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string MemberStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string Channel { get; set; }
        public string CreateUpdateTimestamp { get; set; }
        public string HouseholdUserId { get; set; }
    }
}
