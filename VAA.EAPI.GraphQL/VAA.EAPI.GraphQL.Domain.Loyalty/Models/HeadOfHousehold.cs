using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class HeadOfHousehold
    {
        [GraphQLDescription("FC number for head of Household")]
        public string? MemberNumber { get; set; }
        [GraphQLDescription("Customer ID for head of Household")]
        public string CustomerId { get; set; }
        [GraphQLDescription("First Name for head of Household")]
        public string? FirstName { get; set; }
        [GraphQLDescription("Last Name for head of Household")]
        public string? LastName { get; set; }
        [GraphQLDescription("Account status for head of Household")]
        public string? MemberStatus { get; set; }
        [GraphQLDescription("Association date for head of Household")]
        public DateTime? AssociationDate { get; set; }
        [GraphQLDescription("Group document ID for head of Household")]
        public string GroupDocId { get; set; }
        [GraphQLDescription("Created By for head of Household")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Created On for head of Household")]
        public DateTime CreatedOn { get; set; }

    }
}
