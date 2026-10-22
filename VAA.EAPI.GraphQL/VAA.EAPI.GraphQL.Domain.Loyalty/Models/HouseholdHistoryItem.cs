using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class HouseholdHistoryItem
    {
        [GraphQLDescription("FC number of Household child Member in Grouphistory")]
        public string? MemberNumber { get; set; }
        [GraphQLDescription("Customer ID number of Household child Member in Grouphistory")]
        public string CustomerId { get; set; }
        [GraphQLDescription("First Name of Household child Member in Grouphistory")]
        public string? FirstName { get; set; }
        [GraphQLDescription("Last Name of Household child Member in Grouphistory")]
        public string? LastName { get; set; }
        [GraphQLDescription("Account status of Household child Member in Grouphistory")]
        public string? MemberStatus { get; set; }
        [GraphQLDescription("Start date of Household child Member in Grouphistory")]
        public DateTime? startDate { get; set; }

        [GraphQLDescription("End date of Household child Member in Grouphistory")]
        public DateTime? endDate { get; set; }
        [GraphQLDescription("relation type in the household for given member")]
        public string? relationshipType { get; set; }
        [GraphQLDescription("Group document ID of Household child Member in Grouphistory")]
        public string GroupDocId { get; set; }
        [GraphQLDescription("Last Updated By of Household child Member in Grouphistory")]
        public string LastUpdatedBy { get; set; }

        [GraphQLDescription("Last Updated On of Household child Member in Grouphistory")]
        public DateTime LastUpdatedOn { get; set; }

        [GraphQLDescription("Created By of Household child Member in Grouphistory")]
        public string CreatedBy { get; set; }

    }
}
