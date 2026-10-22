using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class LoyaltyMemberManagementProvider : ILoyaltyMemberManagementProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly static Dictionary<string, string> loyaltyMemberManagementHeaders = new Dictionary<string, string>()
        {
            { Common.Constants.Application.OcpKeyHeader,  ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey)}
        };

        public LoyaltyMemberManagementProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<StatementTextResponse> GenerateStatementText(StatementTextrequest stmtRequest)
        {
            string generateStmtUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.GenerateStatementTextEndPoint}";

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            return (await _connector.SendAsync<StatementTextResponse>(generateStmtUri, loyaltyMemberManagementHeaders, token, HttpMethod.Post, Json.Serialize(stmtRequest))).Response;

        }

    }
}
