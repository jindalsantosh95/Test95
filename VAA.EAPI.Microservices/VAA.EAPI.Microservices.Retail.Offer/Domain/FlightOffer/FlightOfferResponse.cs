using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer
{
    public class FlightOfferResponse
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public int TotalOffers { get => Offers?.Count() ?? 0; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public DataList DataList { get; set; }
    }
}
