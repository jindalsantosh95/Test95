using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData
{
    public class ProductsEventData
    {
        public IEnumerable<Product> Products { get; set; }
    }
}
