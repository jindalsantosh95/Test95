using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance
{
    public class InsuranceQuote
    {
        public InsuranceQuoteStatus Status { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
        public IEnumerable<Product> Products { get; set; }

    }
}