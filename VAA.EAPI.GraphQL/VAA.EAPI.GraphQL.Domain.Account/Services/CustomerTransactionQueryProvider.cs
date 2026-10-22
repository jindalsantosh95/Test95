using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class CustomerTransactionQueryProvider<T> : CustomerTransactionsProviderBase, ICustomerTransactionQueryProvider<T>
    {
        public CustomerTransactionQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<T> CustomerTransactionQuery(string url, string customerId)
        {
            string uri = url.Replace("{{customerId}}", customerId);

            return (await _connector.GetAsync<T>(uri, GetHeaders(), await GetToken())).Response;
        }

        public async Task<T> CustomerTransactionQueryCustomerProfile(string url, string customerId)
        {
            return (await _connector.GetAsync<T>($"{url}{Common.Constants.Application.CustomerProfileEndPoint}".Replace("{CustomerId}", customerId), GetHeaders(), await GetToken())).Response;
        }

        public async Task<T> CustomerTransactionUpdateAddressHouseHold(string url, string customerId)
        {
            return (await _connector.GetAsync<T>($"{url}{Common.Constants.Application.GroupDocumentEndPoint}".Replace("{customerId}", customerId), GetHeaders(), await GetToken())).Response;
        }

        public async Task<T> CustomerTransactionCreateHouseHoldInviteDoc(string url, string groupName, CustomerDTO leaderDoc, CustomerDTO inviteeDoc)
        {
            return (await _connector.SendAsync<T>($"{url}{Common.Constants.Application.CreateHouseHoldInviteDoc}", GetHeaders(), await GetToken(), HttpMethod.Post, Json.Serialize(new InviteHouseHoldCreateDocRequest
            {
                GroupName = groupName,
                InviterMemberId = leaderDoc.Customer?.LoyaltyNumber,
                InviteeMemberId = inviteeDoc.Customer?.LoyaltyNumber,
                InviteeEmail = inviteeDoc.Customer?.ContactDetails.EmailDetails.First(x => x.PreferredFlag == "Y").emailId,
                InviteeFirstName = inviteeDoc.Customer?.FirstName,
                InviteeLastName = inviteeDoc.Customer?.LastName,
                InviterFirstName = leaderDoc.Customer?.FirstName,
            }))).Response;
        }

        public async Task<T> CustomerTransactionDeleteHouseHoldInviteDoc(string url, string leaderFcNumber, string inviteeFcNumber)
        {
            return (await _connector.SendAsync<T>($"{url}{Common.Constants.Application.DeleteHouseHoldInviteDoc}", GetHeaders(), await GetToken(), HttpMethod.Post, Json.Serialize(new InviteHouseHoldDeleteDocRequest
            {
                InviterMemberId = leaderFcNumber,
                InviteeMemberId = inviteeFcNumber,
            }))).Response;
        }

        public async Task<T> CustomerTransactionGetInviteDocInvitee(string url, string memberNumber)
        {
            return (await _connector.GetAsync<T>($"{url}{Common.Constants.Application.GetHouseHoldInviteDocInvitee}".Replace("{inviteeMemberNumber}", memberNumber), GetHeaders(), await GetToken())).Response;
        }

        public async Task<T> CustomerTransactionGetInviteDocInviter(string url, string memberNumber)
        {
            return (await _connector.GetAsync<T>($"{url}{Common.Constants.Application.GetHouseHoldInviteDocInviter}".Replace("{inviterMemberNumber}", memberNumber), GetHeaders(), await GetToken())).Response;
        }

        public async Task<T> CreateTransaction(TransactionRequest txnRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);

            return (await _connector.SendAsync<T>($"{uri}{Common.Constants.Application.CreateTransactionEndPoint}".Replace("{CustomerId}", txnRequest.TransactionDocument.CustomerId), GetHeaders(), await GetToken(), HttpMethod.Post, Common.Serialisation.Json.Serialize(txnRequest))).Response;
        }

        public async Task<T> GetMembershipChangeDocByCustomerID(string customerID)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);

            return (await _connector.SendAsync<T>($"{uri}{Common.Constants.Application.MembershipChangeEndpoint}".Replace("{customerID}", customerID), GetHeaders(), await GetToken(), HttpMethod.Get, customerID)).Response;

        }
    }
}
