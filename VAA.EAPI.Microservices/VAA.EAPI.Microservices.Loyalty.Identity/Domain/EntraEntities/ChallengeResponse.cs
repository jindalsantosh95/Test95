using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class ChallengeResponse
    {
        public int Interval { get; set; }
        public string ContinuationToken { get; set; }
        public string ChallengeType { get; set; }
        public string BindingMethod { get; set; }
        public string ChallengeChannel { get; set; }
        public string ChallengeTargetLabel { get; set; }
        public int CodeLength { get; set; }
    }
}
