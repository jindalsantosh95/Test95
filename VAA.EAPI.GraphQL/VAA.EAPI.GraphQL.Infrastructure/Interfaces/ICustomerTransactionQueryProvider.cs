using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface ICustomerTransactionQueryProvider<T>
    {
        Task<T> CustomerTransactionQuery(string url, string customerId);
        Task<T> CustomerTransactionQueryCustomerProfile(string url, string customerId);
        Task<T> CustomerTransactionUpdateAddressHouseHold(string url, string customerId);
        Task<T> CustomerTransactionCreateHouseHoldInviteDoc(string url, string groupName, CustomerDTO leaderFcNumber, CustomerDTO inviteeFcNumber);
        Task<T> CustomerTransactionDeleteHouseHoldInviteDoc(string url, string leaderFcNumber, string inviteeFcNumber);
        Task<T> CustomerTransactionGetInviteDocInvitee(string url, string memberNumber);
        Task<T> CustomerTransactionGetInviteDocInviter(string url, string memberNumber);
        Task<T> CreateTransaction(TransactionRequest transactionRequest);
        Task<T> GetMembershipChangeDocByCustomerID(string customerId);
    }
}
