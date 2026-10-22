using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class ThresholdValueMapper
    {
        public static IEnumerable<TierThresholdValues> MapThresholdData(IEnumerable<MemberAccountReferenceData> thresholdValues)
        {
            return thresholdValues.Select(item => (TierThresholdValues)item);
        }
    }
}
