using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group
{
    public class GroupDocumentResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<GroupDocument> GroupDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public GroupDocumentResponse()
        {
            // Required for Serialisation
        }

        public GroupDocumentResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of ContactNoteDTO (Cant think of a more efficient way)
            IEnumerable<GroupDocumentDTO> dtoList = Json.Deserialize<IEnumerable<GroupDocumentDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            GroupDocuments = dtoList.Select(dto => new GroupDocument(dto));
        }
    }
}

