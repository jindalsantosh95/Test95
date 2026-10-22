using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch
{
    public class PriceItems
    {
        public string Ptc { get; set; }
        public List<Price> SalePrice { get; set; }

    }
}
