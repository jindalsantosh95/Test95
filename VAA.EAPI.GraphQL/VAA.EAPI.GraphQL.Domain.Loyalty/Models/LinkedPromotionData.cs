using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class LinkedPromotionData
    {
        [GraphQLDescription("The Promotion linked to the Campaign")]
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerName { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
