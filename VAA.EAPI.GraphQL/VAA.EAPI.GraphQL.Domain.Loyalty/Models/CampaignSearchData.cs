using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignSearchData
    {
        [GraphQLDescription("The List of Campaign")]
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CampaignCode { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string CampaignPurpose { get; set; }
        public string RegistrationRequired { get; set; }
        public string LinkToTemplate { get; set; }
        public LinkedPromotionData Promotion { get; set; }
    }
}
