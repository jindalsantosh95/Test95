using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class ContactNote
    {
        [GraphQLDescription("The internal ID of the contact note")]
        public string Id { get; set; }
        [GraphQLDescription("Contact note grouping code")]
        public string Group { get; set; }
        [GraphQLDescription("Contact note type code")]
        public string Type { get; set; }
        [GraphQLDescription("Contact note text")]
        public string Text { get; set; }
        [GraphQLDescription("User who initally created the contact note")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Date/Time when contact note was initially created")]
        public DateTime? CreatedOn { get; set; }
        [GraphQLDescription("User who most recently updated the contact note")]
        public string LastUpdatedBy { get; set; }
        [GraphQLDescription("Date/Time when contact note was most recently updated")]
        public DateTime? LastUpdatedOn { get; set; }
    }
}
