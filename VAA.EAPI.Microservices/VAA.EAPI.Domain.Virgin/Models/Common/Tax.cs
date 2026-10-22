using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class Tax
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string CountryCode { get; set; }
        public List<CollectionPoint> CollectionPoints { get; set; }
        public bool Refundable { get; set; }


    }
}
