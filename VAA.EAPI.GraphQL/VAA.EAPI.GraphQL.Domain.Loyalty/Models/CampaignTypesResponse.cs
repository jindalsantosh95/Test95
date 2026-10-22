using HotChocolate;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignTypesResponse
    {
        [GraphQLDescription("The List of Campaign Types")]
        public List<string> CampaignTypes { get; set; }
    }
}
