using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class InviteHouseHoldRepoQuery : CustomerTransactionRepoQuery, IRequest<object>
    {
        public string InviteeMemberNumber { get; set; }

        public InviteHouseHoldRepoQuery(string inviteeMemberNumber)
        {
            InviteeMemberNumber = inviteeMemberNumber;
        }
    }
}
