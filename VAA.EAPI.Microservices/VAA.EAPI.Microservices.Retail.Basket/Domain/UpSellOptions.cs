using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class UpSellOptions
    {
        public string Selected { get; set; }
        public IEnumerable<string> Options { get; set; }
        public string Type { get; set; }
    }
}