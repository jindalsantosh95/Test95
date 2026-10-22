using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class GroupDocByHeadCustomerIdRepoQuery : CustomerTransactionRepoQuery, IRequest<IEnumerable<object>>
    {
        public string HeadCustomerId { set; get; }

        public GroupDocByHeadCustomerIdRepoQuery(string headCustomerId)
        {
            HeadCustomerId = headCustomerId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}
