using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class GroupDocRepoQuery : CustomerTransactionRepoQuery, IRequest<IEnumerable<object>>
    {
        public string GroupId { set; get; }

        public GroupDocRepoQuery(string groupId)
        {
            GroupId = groupId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}
