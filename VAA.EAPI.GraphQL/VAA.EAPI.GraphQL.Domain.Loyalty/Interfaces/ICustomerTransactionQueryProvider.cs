using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;


namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICustomerTransactionQueryProvider
    {
        Task<Models.AwardResponse> GetAwardDetails(string customerId);
        Task<TransactionResponse<Transaction>> CustomerTransactionQuery(string url, string customerId);
        Task<AwardMutationResponse> MutateAward(AwardRequest request, HttpMethod methodType);
        Task<FlyingClubMemberMutationResponse> UpdateFlyingClubMember(FlyingClubMemberRequest request);
        Task<FlyingClubMemberAPIResponse> GetFlyingClubMember(string customerId);
        Task<TransactionResponse<Transaction>> RelatedTransactionsQuery(string customerId, string transactionId);
        Task<MemberCustomAttributesResponse> GetMemberCustomAttributes(string customerId);
    }
}
