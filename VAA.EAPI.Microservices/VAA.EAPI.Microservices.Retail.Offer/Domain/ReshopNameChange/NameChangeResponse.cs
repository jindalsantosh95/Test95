using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.ReshopNameChange
{
    public class NameChangeResponse
    {
        public string OrderId { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public int TotalOffers { get; set; }
        public List<OfferDetails> Offers { get; set; }
        public DataList DataList { get; set; }
        public string MetaPartnerId { get; set; }
    }
}

