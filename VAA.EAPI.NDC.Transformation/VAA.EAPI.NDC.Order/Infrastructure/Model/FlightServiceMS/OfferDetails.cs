using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS
{
    public class OfferDetails
    {
        public string OfferId { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
    }
}
