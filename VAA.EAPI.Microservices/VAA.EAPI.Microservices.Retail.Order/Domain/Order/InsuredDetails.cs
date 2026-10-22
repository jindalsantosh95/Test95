using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class InsuredDetails
    {
        public IEnumerable<Insured> Insured { get; set; }
    }
}