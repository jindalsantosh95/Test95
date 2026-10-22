using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PartnersResponse
    {
        public List<PartnerProfile> Partners { get; set; }

        public PartnersResponse() { }
    }
}
