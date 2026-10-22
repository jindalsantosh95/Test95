using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.ValidateUniqueId
{
    public class ValidateUniqueIdResponse
    {
        public bool Valid { get; set; }
        public bool Claimed { get; set; }
        public string FcNumber { get; set; }
        public DateTime? ClaimedDateTime { get; set; }
    }
}
