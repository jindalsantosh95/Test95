using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class CustomerTransactionRepoQuery : IRequest<IEnumerable<object>>
    {
        public string CustomerId { set; get; }
        public string DocType { set; get; }
        public ICollection<string> FieldList { get; set; } = new List<string>();

        public CustomerTransactionRepoQuery() { }

        public CustomerTransactionRepoQuery(string customerId, string docType, ICollection<string> fieldList)
        {
            CustomerId = customerId;
            DocType = docType;
            FieldList = fieldList;
        }
    }
}
