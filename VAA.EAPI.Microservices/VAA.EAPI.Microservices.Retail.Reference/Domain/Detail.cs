using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class Detail
    {
        public IEnumerable<string> Disclaimers { get; set; }
        public GenericFeatures GenericFeatures { get; set; }
        public string HeroImageURI { get; set; }
        public IEnumerable<string> Description { get; set; }
        public string Title { get; set; }
        public KeyFeatures keyFeatures { get; set; }
    }
}
