using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign
{
    public class CampaignUnassignResponse
    {
        public string Value { get; set; }
        public Status Status { get; set; }
        public string RequestId { get; set; }
        public List<Error> Errors { get; set; }
    }
}