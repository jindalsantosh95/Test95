using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class AwardAuditHistoryQueryProvider : CustomerTransactionsProviderBase, IAwardAuditHistoryQueryProvider
    {
        public AwardAuditHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<AwardAuditHistoryResponse> GetAwardAuditHistory(string customerId)
        {
            string awardAuditHistoryUri = $"{ConfigReader.Get(Common.Constants.Environment.AuditMsUrl)}{Common.Constants.Application.AuditMSAwardAuditHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);

            var auditMStoken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AuditAppId));

            Dictionary<string, string> auditMSHeaders = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AuditOcpKey) }
            };

            return new AwardAuditHistoryResponse()
            {
                AwardAuditHistory = (await _connector.GetAsync<AwardAuditHistoryResponse>(awardAuditHistoryUri, auditMSHeaders, auditMStoken)).Response.AwardAuditHistory
            };
        }
    }
}