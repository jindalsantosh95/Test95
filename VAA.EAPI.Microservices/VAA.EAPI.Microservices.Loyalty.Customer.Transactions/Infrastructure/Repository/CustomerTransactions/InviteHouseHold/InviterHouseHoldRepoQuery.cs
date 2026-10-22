using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.InviteHouseHold
{
    public class InviterHouseHoldRepoQuery : CustomerTransactionRepoQuery, IRequest<object>
    {
        public string InviterMemberNumber { get; set; }
        public string SQLQuery { get; set; }
        public string InviteParameter { get; set; }

        public InviterHouseHoldRepoQuery(string inviterMemberNumber, string sqlQuery, string inviteParameter)
        {
            InviterMemberNumber = inviterMemberNumber;
            SQLQuery = sqlQuery;
            InviteParameter = inviteParameter;
        }
    }
}
