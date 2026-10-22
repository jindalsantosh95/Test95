using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PreferenceAuditHistoryQueryProvider : CustomerTransactionsProviderBase, IPreferenceAuditHistoryQueryProvider
    {
        public PreferenceAuditHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<PreferenceAuditHistoryResponse> GetPreferenceAuditHistory(string customerId)
        {
            string preferenceAuditHistoryUri = $"{ConfigReader.Get(Common.Constants.Environment.AuditMsUrl)}{Common.Constants.Application.AuditMSPreferenceAuditHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);

            // Get a AuditMS token
            var auditMStoken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AuditAppId));

            // Retrieve PreferenceAuditHistory
            Dictionary<string, string> auditMSHeaders = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AuditOcpKey) }
            };

            return new PreferenceAuditHistoryResponse { PreferenceHistoryAudit = (await _connector.GetAsync<PreferenceAuditHistoryResponse>(preferenceAuditHistoryUri, auditMSHeaders, auditMStoken)).Response.PreferenceHistoryAudit };
        }
    }
}