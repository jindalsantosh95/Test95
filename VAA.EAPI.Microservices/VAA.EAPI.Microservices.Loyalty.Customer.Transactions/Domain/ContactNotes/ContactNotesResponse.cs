using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes
{
    public class ContactNotesResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<ContactNote> ContactNotes { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public ContactNotesResponse()
        {
            // Required for Serialisation
        }

        public ContactNotesResponse(string customerId, IEnumerable<object> transactions)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of ContactNoteDTO (Cant think of a more efficient way)
            IEnumerable<ContactNoteDTO> dtoList = Json.Deserialize<IEnumerable<ContactNoteDTO>>(Json.Serialize(transactions));

            // Convert from DTO to Domain
            ContactNotes = dtoList.Select(dto => new ContactNote(dto));
        }
    }
}
