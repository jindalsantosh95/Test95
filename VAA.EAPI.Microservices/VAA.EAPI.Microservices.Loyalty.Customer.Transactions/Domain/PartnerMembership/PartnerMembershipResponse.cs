using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership
{
    public class PartnerMembershipResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<PartnerMembership> PartnerMembershipDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public PartnerMembershipResponse()
        {
            // Required for Serialisation
        }

        public PartnerMembershipResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of FlyingClubMemberDTO 
            IEnumerable<PartnerMembershipDTO> dtoList = Json.Deserialize<IEnumerable<PartnerMembershipDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            PartnerMembershipDocuments = dtoList.Select(dto => (PartnerMembership)dto);

        }
    }
}
