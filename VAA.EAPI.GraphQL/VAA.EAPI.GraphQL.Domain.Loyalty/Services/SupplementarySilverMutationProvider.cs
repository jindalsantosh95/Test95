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
    public sealed class SupplementarySilverMutationProvider : ISupplementarySilverMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public SupplementarySilverMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<string> SupplementarySilverDelinking(SupplementarySilverDelinkingRequest supplementarySilverDelinkingRequest)
        {
            string supplementarySilverDelinkingUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.SupplementarySilverDelinkingEndPoint}";

            //// Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke Supplementary Silver Delinking
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<ResponseMessage>(supplementarySilverDelinkingUri, headers, token, HttpMethod.Post, Json.Serialize(supplementarySilverDelinkingRequest))).RawResponse;

        }

        public async Task<string> SupplementarySilverLinking(SupplementarySilverLinkingRequest supplementarySilverLinkingRequest)
        {
            string supplementarySilverLinkingUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.SupplementarySilverlinkingEndPoint}";

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke Supplementary Silver Linkinglinking
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<ResponseMessage>(supplementarySilverLinkingUri, headers, token, HttpMethod.Post, Json.Serialize(supplementarySilverLinkingRequest))).RawResponse;
        }
    }
}
