using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class PartnerCodeMapper
    {
        public static IEnumerable<PartnerProfileDetails> MapPartnerCodeData(IEnumerable<PartnerProfile> partnerProfiles)
        {
            return partnerProfiles.Select(item => (PartnerProfileDetails)item);
        }
    }
}
