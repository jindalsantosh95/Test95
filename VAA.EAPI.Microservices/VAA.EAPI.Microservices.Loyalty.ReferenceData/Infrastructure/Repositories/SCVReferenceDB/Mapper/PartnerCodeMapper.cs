using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class PartnerCodeMapper
    {
        public static IEnumerable<PartnerProfileDetails> MapPartnerCodeData(IEnumerable<PartnerProfile> partnerProfiles)
        {
            return partnerProfiles.Select(item => (PartnerProfileDetails)item);
        }
    }
}
