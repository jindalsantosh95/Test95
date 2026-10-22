using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PromotionEnrolledMembersQueryProvider : CustomerTransactionsProviderBase, IPromotionEnrolledMembersQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        public PromotionEnrolledMembersQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PromotionEnrolledMemberResponse> Query(string promotionId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.PartnersRetrievalEndPoint}".Replace("{promotionId}", promotionId);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) }
                            };
            var promotionEnrolledMemberResponse = await _connector.GetAsync<PromotionEnrolledMemberResponse>(uri, headers, token);
            return promotionEnrolledMemberResponse.Response;
        }
    }
}
