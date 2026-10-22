using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class PricingQualifiers
    {
        public IEnumerable<PassengerType> PassengerType { get; set; }
        public IEnumerable<PriceQuote> PriceQuote { get; set; }
    }
}
