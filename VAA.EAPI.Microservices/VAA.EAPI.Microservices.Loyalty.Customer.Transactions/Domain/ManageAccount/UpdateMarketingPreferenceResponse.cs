using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount
{
    public class UpdateMarketingPreferenceResponse
    {
        public bool PreferenceValue { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
