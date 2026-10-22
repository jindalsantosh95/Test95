using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference
{
    public class ProductDataPayload
    {
        public IEnumerable<Product> Products { get; set; }

    }
}
