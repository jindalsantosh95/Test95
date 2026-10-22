using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership
{
    public class TargetedMembershipResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<TargetedMembership> TargetedMembershipDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public TargetedMembershipResponse()
        {
            // Required for Serialisation
        }

        public TargetedMembershipResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of ContactNoteDTO (Cant think of a more efficient way)
            IEnumerable<TargetedMembershipDTO> dtoList = Json.Deserialize<IEnumerable<TargetedMembershipDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            TargetedMembershipDocuments = dtoList.Select(dto => new TargetedMembership(dto));
        }
    }
}
