using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS
{
    public class CancelReshopResponse
    {
        public string OrderId { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public IEnumerable<ReshopOffer> Offers { get; set; }
        public DataList DataList { get; set; }
        public bool IsRfc { get; set; }
    }
}
