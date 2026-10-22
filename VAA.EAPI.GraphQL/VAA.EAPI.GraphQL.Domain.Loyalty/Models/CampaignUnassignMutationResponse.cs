using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignUnassignMutationResponse
    {
        public string RequestId { get; set; }
        public List<ErrorDetails> Errors { get; set; }
        public string Value { get; set; }
        public Status Status { get; set; }
    }
}
