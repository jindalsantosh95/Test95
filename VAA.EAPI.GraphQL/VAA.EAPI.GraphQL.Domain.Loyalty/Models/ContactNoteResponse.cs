using HotChocolate;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class ContactNoteResponse
    {
        [GraphQLDescription("List of Contact Notes")]
        public IEnumerable<ContactNote> ContactNotes { get; set; }
        public ContactNoteResponse() { }
    }
}