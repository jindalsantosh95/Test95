using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class FlightsearchRequest
    {
        public IEnumerable<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public string BrandId { get; set; }
        public bool isAwardSearch { get; set; }
        public string PromoCode { get; set; }
    }
}
