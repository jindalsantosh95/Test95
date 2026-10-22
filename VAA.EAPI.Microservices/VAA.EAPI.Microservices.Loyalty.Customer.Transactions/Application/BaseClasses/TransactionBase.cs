using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses
{
    public class TransactionBase
    {
        public string CustomerId { get; set; }
        public string MemberId { get; set; }
        internal string DocType { get; set; }
        public ICollection<string> FieldList { get; set; } = new List<string>();

        public TransactionBase()
        {
            // Required for Serialization
        }
    }

}
