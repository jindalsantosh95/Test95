using System.Collections.Generic;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember
{
    public class UpgradeCustomerDetailsMS : EnrolCustomerRequest
    {
        public string ProgramName { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public string CampaignCode { get; set; }
        public string ChannelUserId { get; set; }
        public GuardianDetails GuardianDetails { get; set; }
        public PartnerMember PartnerMember { get; set; }
        public IEnumerable<CustomerAttribute> CustomerAttribute { get; set; }
    }
}
