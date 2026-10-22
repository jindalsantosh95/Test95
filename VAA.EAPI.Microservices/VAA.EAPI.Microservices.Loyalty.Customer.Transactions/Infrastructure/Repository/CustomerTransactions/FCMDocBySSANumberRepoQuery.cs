using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class FCMDocBySSANumberRepoQuery : CustomerTransactionRepoQuery, IRequest<IEnumerable<object>>
    {
        public string SSAMemberNumber { set; get; }

        public FCMDocBySSANumberRepoQuery(string ssaMemberNumber)
        {
            SSAMemberNumber = ssaMemberNumber;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.FlyingClubMemberDocType);
        }
    }
}