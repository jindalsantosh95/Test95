using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class CheckedBaggage
    {
        public string ProductType { get; set; }
        public IEnumerable<PsgBaggageInfo> PassengerBaggage { get; set; }

    }
}
