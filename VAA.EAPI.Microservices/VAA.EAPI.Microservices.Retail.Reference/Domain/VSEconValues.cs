using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class VSEconValues
    {
        public Upsell Upsell { get; set; }
        public Detail Detail { get; set; }
        public string Title { get; set; }
        public IEnumerable<KeyFeatures> KeyFeatures { get; set; }
    }
}
