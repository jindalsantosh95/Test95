using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class HouseholdMember
    {
        [GraphQLDescription("FC number of Household child Member")]
        public string? MemberNumber { get; set; }
        [GraphQLDescription("Customer ID number of Household child Member")]
        public string CustomerId { get; set; }
        [GraphQLDescription("First Name of Household child Member")]
        public string? FirstName { get; set; }
        [GraphQLDescription("Last Name of Household child Member")]
        public string? LastName { get; set; }
        [GraphQLDescription("Account status of Household child Member")]
        public string? MemberStatus { get; set; }
        [GraphQLDescription("Association date of Household child Member")]
        public DateTime? AssociationDate { get; set; }
        [GraphQLDescription("relation type in the household for given member")]
        public string? RelationshipType { get; set; }
        [GraphQLDescription("Group document ID of Household child Member")]
        public string GroupDocId { get; set; }
        [GraphQLDescription("Last Updated By of Household child Member")]
        public string LastUpdatedBy { get; set; }
        [GraphQLDescription("Last Updated On of Household child Member")]
        public DateTime LastUpdatedOn { get; set; }
        [GraphQLDescription("Created By of Household child Member")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Created On of Household child Member")]
        public DateTime CreatedOn { get; set; }
    }
}
