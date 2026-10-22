using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerTransactionQueryProvider : CustomerTransactionsProviderBase, ICustomerTransactionQueryProvider
    {
        public CustomerTransactionQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<Models.AwardResponse> GetAwardDetails(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{customerId}/awards";

            return (await _connector.GetAsync<Models.AwardResponse>(uri, GetHeaders(), await GetToken())).Response;
        }

        public async Task<TransactionResponse<Transaction>> CustomerTransactionQuery(string url, string customerId)
        {
            string uri = url.Replace("{CustomerId}", customerId);

            return (await _connector.GetAsync<TransactionResponse<Transaction>>(uri, GetHeaders(), await GetToken())).Response;
        }

        public async Task<AwardMutationResponse> MutateAward(AwardRequest request, HttpMethod methodType)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{request.award.CustomerId}/award";

            return (await _connector.SendAsync<AwardMutationResponse>($"{uri}", GetHeaders(), await GetToken(), methodType, Common.Serialisation.Json.Serialize(request))).Response;
        }

        public async Task<FlyingClubMemberMutationResponse> UpdateFlyingClubMember(FlyingClubMemberRequest request)
        {
            string uri = (ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).Trim('/') + Common.Constants.Application.UpdateFlyingClubMemberEndPoint).Replace("{customerID}", request.FlyingClubMemberDocument.CustomerId);

            return (await _connector.SendAsync<Models.MemberMerge.FlyingClubMemberMutationResponse>(uri, GetHeaders(), await GetToken(), HttpMethod.Patch, Common.Serialisation.Json.Serialize(request))).Response;
        }

        public async Task<FlyingClubMemberAPIResponse> GetFlyingClubMember(string customerId)
        {
            string uri = (ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl) + Common.Constants.Application.GetFcMemberDetails).Replace("{{customerId}}", customerId);

            return (await _connector.GetAsync<FlyingClubMemberAPIResponse>(uri, GetHeaders(), await GetToken())).Response;
        }

        public async Task<TransactionResponse<Transaction>> RelatedTransactionsQuery(string customerId, string transactionId)
        {
            string uri = (ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl) + Common.Constants.Application.GetRelatedTransactions).Replace("{customerID}", customerId).Replace("{transactionId}", transactionId);

            return (await _connector.GetAsync<TransactionResponse<Transaction>>(uri, GetHeaders(), await GetToken())).Response;
        }

        public async Task<MemberCustomAttributesResponse> GetMemberCustomAttributes(string customerId)
        {
            string uri = (ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl) + Common.Constants.Application.GetMemberCustomAttributes).Replace("{customerID}", customerId);

            return (await _connector.GetAsync<MemberCustomAttributesResponse>(uri, GetHeaders(), await GetToken())).Response;
        }
    }
}
