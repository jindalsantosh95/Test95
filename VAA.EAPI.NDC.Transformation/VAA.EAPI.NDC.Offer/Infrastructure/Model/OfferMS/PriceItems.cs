using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class PriceItems
    {
        public string Ptc { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
    }
}
