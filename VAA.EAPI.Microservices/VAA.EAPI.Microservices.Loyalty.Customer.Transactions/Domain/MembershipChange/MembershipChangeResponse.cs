using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange
{
    public class MembershipChangeResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<MembershipChange> MembershipChangeDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public MembershipChangeResponse()
        {
            // Required for Serialisation
        }
        public MembershipChangeResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of ContactNoteDTO (Cant think of a more efficient way)
            IEnumerable<MembershipChangeDTO> dtoList = Json.Deserialize<IEnumerable<MembershipChangeDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            MembershipChangeDocuments = dtoList.Select(dto => new MembershipChange(dto));
        }
    }
}
