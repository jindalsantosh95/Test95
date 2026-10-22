using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MemberCampaign
    {
        [GraphQLDescription("The internal ID of the document")]
        public string Id { get; set; }
        [GraphQLDescription("Date/Time targeted member document was assigned to customer")]
        public DateTime? AssignmentDate { get; set; }
        [GraphQLDescription("Date/Time targeted member document was assigned to customer")]
        public DateTime? ContactedDate { get; set; }
        [GraphQLDescription("Campaign End Date")]
        public DateTime? CompletedDate { get; set; }
        [GraphQLDescription("Campaign document Status")]
        public string DocumentStatusCode { get; set; }
        [GraphQLDescription("Comments for the document")]
        public string Comments { get; set; }
        [GraphQLDescription("User who initally created the document")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Date/Time document was initially created")]
        public DateTime? CreatedOn { get; set; }
        [GraphQLDescription("User who most recently updated the document")]
        public string LastUpdatedBy { get; set; }
        [GraphQLDescription("Date/Time document was last updated")]
        public DateTime? LastUpdatedOn { get; set; }
        [GraphQLDescription("Capmaign ID")]
        public string CampaignId { get; set; }
        [GraphQLDescription("Customer ID this document is assigned to")]
        public string CustomerId { get; set; }
        [GraphQLDescription("Campaign Name")]
        public string CampaignName { get; set; }
        [GraphQLDescription("Campaign Code")]
        public string CampaignCode { get; set; }
        [GraphQLDescription("Campaign Start Date")]
        public DateTime StartDate { get; set; }
        [GraphQLDescription("Campaign End Date")]
        public DateTime EndDate { get; set; }
        [GraphQLDescription("Campaign Status")]
        public string Status { get; set; }
        [GraphQLDescription("Campaign Type")]
        public string Type { get; set; }
        [GraphQLDescription("Campaign Description")]
        public string Description { get; set; }
    }
}
