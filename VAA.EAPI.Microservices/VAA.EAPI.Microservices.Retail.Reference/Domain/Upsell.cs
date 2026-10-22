using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class Upsell
    {
        public string HeroImageURI { get; set; }
        public IEnumerable<KeyFeatures> keyFeatures { get; set; }
    }
}
