using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public sealed class HouseHoldMutationProvider : IHouseHoldMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public HouseHoldMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<string> HouseHoldLinking(HouseHoldLinkingRequest houseHoldLinkingRequest)
        {
            string houseHoldLinkingUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.HouseholdLinkingEndPoint}";

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke household linking
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<ResponseMessage>(houseHoldLinkingUri, headers, token, HttpMethod.Post, Json.Serialize(houseHoldLinkingRequest))).RawResponse;
        }

        public async Task<string> HouseHoldDelinking(HouseHoldDelinkingRequest houseHoldDelinkingRequest)
        {
            string houseHoldDelinkingUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.HouseholdDelinkingEndPoint}";

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke household Delinking
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<ResponseMessage>(houseHoldDelinkingUri, headers, token, HttpMethod.Post, Json.Serialize(houseHoldDelinkingRequest))).RawResponse;
        }
    }
}
