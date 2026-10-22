using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class SupplementarySilverAccountQueryProvider : ISupplementarySilverAccountQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public SupplementarySilverAccountQueryProvider(IOAuthTokenProvider tokenProvider,
            IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<SupplementarySilverAccountResponse> Query(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.SSAMemberAccountRetrievalEndPoint}".Replace("{customerId}", customerId);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) }
                            };
            var apiResponse = await _connector.GetAsync<SupplementarySilverAccountResponse>(uri, headers, token);
            return apiResponse.Response;
        }
    }
}