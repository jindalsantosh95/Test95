using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class ProductIndentifiers : BaseDomain
    {
        public IEnumerable<ProductValue> Values { get; set; }
    }
}
