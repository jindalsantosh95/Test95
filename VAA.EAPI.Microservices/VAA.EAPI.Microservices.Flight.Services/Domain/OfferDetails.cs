using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class OfferDetails
    {
        public string OfferId { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
    }
}
