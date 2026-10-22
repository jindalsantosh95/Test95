using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class TargetedMembership
    {
        [GraphQLDescription("The internal ID of the document")]
        public string Id { get; set; }
        [GraphQLDescription("User who initally created the document")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Date/Time document was initially created")]
        public DateTime? CreatedOn { get; set; }
        [GraphQLDescription("User who most recently updated the document")]
        public string LastUpdatedBy { get; set; }
        [GraphQLDescription("Date/Time document was last updated")]
        public DateTime? LastUpdatedOn { get; set; }
        [GraphQLDescription("Customer ID this document is assigned to")]
        public string CustomerId { get; set; }
        [GraphQLDescription("Comments for the document")]
        public string Comments { get; set; }
        [GraphQLDescription("Date/Time targeted member document was assigned to customer")]
        public DateTime? AssignmentDate { get; set; }
        [GraphQLDescription("Campaign End Date")]
        public DateTime? CompletedDate { get; set; }
        [GraphQLDescription("Date/Time targeted member document was assigned to customer")]
        public DateTime? ContactedDate { get; set; }
        [GraphQLDescription("Capmaign ID")]
        public string ProfilingCampaignSourceId { get; set; }
        [GraphQLDescription("Campaign Status")]
        public string StatusCode { get; set; }
        [GraphQLDescription("The internal ID of the document")]
        public string TargetedMembershipId { get; set; }

    }

}
