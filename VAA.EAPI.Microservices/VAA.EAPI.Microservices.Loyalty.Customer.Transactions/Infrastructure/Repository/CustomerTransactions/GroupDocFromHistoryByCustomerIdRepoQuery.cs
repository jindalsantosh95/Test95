using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class GroupDocFromHistoryByCustomerIdRepoQuery : CustomerTransactionRepoQuery, IRequest<IEnumerable<object>>
    {
        public string ChildCustomerId { set; get; }

        public GroupDocFromHistoryByCustomerIdRepoQuery(string childCustomerId)
        {
            ChildCustomerId = childCustomerId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}

