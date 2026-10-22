using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities
{
    public class AmperityDetails
    {
        public string Id { get; set; }        
        public string IndexId { get; set; }
        public AmperityAttributes  Attributes { get; set; }

        public static explicit operator AmperityDetails(AmperityResponse amperityResponse)
        {
            if (amperityResponse == null)
                return null;
            return new()
            {
                Id = amperityResponse.Id,
                IndexId = amperityResponse.IndexId,
                Attributes = new AmperityAttributes()
                {
                    AmperityId = amperityResponse.Attributes.AmperityId,
                    LoyaltyId = amperityResponse.Attributes.LoyaltyId
                }
            };            
        }
    }
}
