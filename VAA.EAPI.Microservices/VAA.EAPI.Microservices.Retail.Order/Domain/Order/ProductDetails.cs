using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class ProductDetails
    {
        public IEnumerable<Product> Product { get; set; }
    }
}