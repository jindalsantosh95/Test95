using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class MonetaryAmounts
    {
        public IEnumerable<object> CommissionRecords { get; set; }
        public IEnumerable<TaxFeeRecord> TaxFeeRecords { get; set; }
    }
}
