using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS
{
    public class OfferResponse
    {
        public string DocType { get; set; }
        public string OrderId { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public int TotalOffers { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
        public DataList DataList { get; set; }
        public bool? IsRfc { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }

    }
}

