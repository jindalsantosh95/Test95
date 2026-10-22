using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class CustomerMutationProvider<T> : ICustomerMutationProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CustomerMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider;
            _connector = connector;
        }

        private readonly static Dictionary<string, string> customerMSHeaders = new()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Environment.CustomerOcpKey) }
        };

        public async Task<T> GetTierResponse(string emailAddress)
        {
            var CustomerUri = $"{ConfigReader.Get(Environment.CustomerMsUrl)}{Application.CheckTierDetailswithEmail}".Replace("{emailId}", emailAddress);

            return (await _connector.SendAsync<T>(CustomerUri, customerMSHeaders, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustomerAppId)), HttpMethod.Post, Json.Serialize(emailAddress))).Response;
        }

        public async Task<string> UpgradeFlyingClub(EnrolmentRequest enrolmentRequest)
        {
            string customerUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.UpgradeFlyingClubEndpoint}";

            return (await _connector.SendAsync<string>(customerUri, customerMSHeaders, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)), HttpMethod.Post, Json.Serialize(enrolmentRequest))).Response;
        }

        public async Task<T> UpdateVaCustomerProfile(VirginAccountCustomerUpdate updateRequest, string fcNumber, string customerId)
        {
            string customerUri = $"{ConfigReader.Get(Environment.CustomerMsUrl)}{Application.VACMemberUpdateEndPoint}";

            return (await _connector.SendAsync<T>(customerUri, customerMSHeaders, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustomerAppId)), HttpMethod.Patch, Json.Serialize((VirginAccountCustomerUpdateRequest)(updateRequest, fcNumber, customerId)))).Response;
        }

    }
}
