using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class AmexCenturionAuditHistoryQueryProvider : CustomerTransactionsProviderBase, IAmexCenturionAuditHistoryQueryProvider
    {
        public AmexCenturionAuditHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<AmexCenturionAuditHistoryResponse> GetAmexCenturionAuditHistory(string customerId)
        {
            string amexCenturionAuditHistoryUri = $"{ConfigReader.Get(Common.Constants.Environment.AuditMsUrl)}{Common.Constants.Application.AuditMSAmexCenturionAuditHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);

            var auditMStoken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AuditAppId));

            Dictionary<string, string> auditMSHeaders = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AuditOcpKey) }
            };

            return new AmexCenturionAuditHistoryResponse()
            {
                AmexCenturionAuditHistory = (await _connector.GetAsync<AmexCenturionAuditHistoryResponse>(amexCenturionAuditHistoryUri, auditMSHeaders, auditMStoken)).Response.AmexCenturionAuditHistory
            };
        }
    }
}