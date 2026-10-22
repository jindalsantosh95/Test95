using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class OfferItem
    {
        public string OfferItemId { get; set; }
        public string OfferItemDesc { get; set; }
        public string SSRCode { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public CurrencyAmount Price { get; set; }
        public IEnumerable<string> PTC { get; set; }

    }
}
