using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.Banner
{
    public class BannerRepoQuery : CustomerTransactionRepoQuery, IRequest<object>
    {
        public string SQLQuery { get; set; }

        public BannerRepoQuery(string customerId, string sqlQuery)
        {
            CustomerId = customerId;
            SQLQuery = sqlQuery;
        }
    }
}
