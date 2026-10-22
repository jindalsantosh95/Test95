using HotChocolate;
using Newtonsoft.Json;
using System.Net.Http;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership
{
    public class PartnerMembershipMutationRequest
    {
        [GraphQLDescription("Partner Membership Id")]
        public string Id { get; set; }
        [GraphQLDescription("Customer Id")]
        public string CustomerId { get; set; }
        [GraphQLDescription("Member Number")]
        public string MemberNumber { get; set; }
        [GraphQLDescription("Partner Id")]
        public string PartnerId { get; set; }
        [GraphQLDescription("Partner Status Code")]
        public string PartnerCode { get; set; }
        [GraphQLDescription("Card Type")]
        public string CardType { get; set; }
        [GraphQLDescription("Start Date")]
        public string StartDate { get; set; }
        [GraphQLDescription("End Date")]
        public string EndDate { get; set; }
        [GraphQLDescription("Created By")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("Last Updated By")]
        public string LastUpdatedBy { get; set; }
        [JsonIgnore]
        public HttpMethod RequestType { get; set; }
    }
}
