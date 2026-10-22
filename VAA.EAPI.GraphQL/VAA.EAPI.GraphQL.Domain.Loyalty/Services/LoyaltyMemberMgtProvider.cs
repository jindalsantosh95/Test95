using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class LoyaltyMemberMgtProvider : ILoyaltyMemberMgtProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public LoyaltyMemberMgtProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<RewardBookingPointCalculationResponse> RetrieveRewardBookingPoints(RewardBookingPointCalculationRequest rewardBookingPointCalculationRequest)
        {
            string retrieveRewardBookingPointsUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.RetrieveRewardBookingPointsEndPoint}";

            //// Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke point calculation
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<RewardBookingPointCalculationResponse>(retrieveRewardBookingPointsUri, headers, token, HttpMethod.Post, Json.Serialize(rewardBookingPointCalculationRequest))).Response;

        }

        public async Task<TransactionStatementTextResponse> GenerateStatetmentText(TransactionStatementTextRequest stmtRequest)
        {
            string generateStmtUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.GenerateStatementTextEndPoint}";

            //// Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            //Invoke statement generation
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };

            return (await _connector.SendAsync<TransactionStatementTextResponse>(generateStmtUri, headers, token, HttpMethod.Post, Json.Serialize(stmtRequest))).Response;

        }
    }
}
