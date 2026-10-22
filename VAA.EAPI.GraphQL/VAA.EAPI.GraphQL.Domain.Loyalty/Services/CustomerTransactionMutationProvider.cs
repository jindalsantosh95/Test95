using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerTransactionMutationProvider<T> : CustomerTransactionsProviderBase, ICustomerTransactionMutationProvider<T>
    {
        public CustomerTransactionMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<T> Mutate(PartnerMembershipRequest request)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);
            string endpoint = (request.RequestType == HttpMethod.Post) ? Common.Constants.Application.CreatePartnermemberMembershipEndPoint : Common.Constants.Application.UpdatePartnermemberMembershipEndPoint;

            return (await _connector.SendAsync<T>($"{uri}{endpoint}".Replace("{customerID}", request.PartnerMembershipDocument.CustomerId), GetHeaders(), await GetToken(), request.RequestType, Common.Serialisation.Json.Serialize(request))).Response;
        }

        public async Task<T> CreateTransaction(TransactionRequest txnRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);
            return (await _connector.SendAsync<T>($"{uri}{Common.Constants.Application.CreateTransactionEndPoint}".Replace("{CustomerId}", txnRequest.TransactionDocument.CustomerId), GetHeaders(), await GetToken(), HttpMethod.Post, Common.Serialisation.Json.Serialize(txnRequest))).Response;
        }

        public async Task<MemberPreferenceUpdateResponse> UpdateVACMemberPreference(MemberPreferenceUpdateRequest updateMemberPreferenceRequest)
        {
            string custTransactionUri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.VACMemberPreferenceUpdateEndPoint}";

            return (await _connector.SendAsync<MemberPreferenceUpdateResponse>(custTransactionUri, GetHeaders(), await GetToken(), HttpMethod.Patch, Json.Serialize(updateMemberPreferenceRequest))).Response;
        }

        public async Task<T> CancelTransaction(TransactionRequest txnRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);
            return (await _connector.SendAsync<T>($"{uri}{Common.Constants.Application.CancelTransactionEndPoint}".Replace("{customerID}", txnRequest.TransactionDocument.CustomerId), GetHeaders(), await GetToken(), HttpMethod.Post, Common.Serialisation.Json.Serialize(txnRequest))).Response;
        }
    }
}
