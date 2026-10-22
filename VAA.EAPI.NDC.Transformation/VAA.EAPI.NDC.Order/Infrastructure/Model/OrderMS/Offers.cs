using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
{
    public class Offers
    {
        public string OfferId { get; set; }
        public string ProductType { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }        
    }
}
