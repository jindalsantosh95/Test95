using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class TaxCode : BaseDomain
    {
        public IEnumerable<CodeValuePair> Values { get; set; }
    }
}
