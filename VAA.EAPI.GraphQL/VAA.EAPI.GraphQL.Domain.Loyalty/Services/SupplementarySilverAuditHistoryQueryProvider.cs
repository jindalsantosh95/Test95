using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class SupplementarySilverAuditHistoryQueryProvider : CustomerTransactionsProviderBase, ISupplementarySilverAuditHistoryQueryProvider
    {
        public SupplementarySilverAuditHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<SupplementarySilverAuditHistoryResponse> GetSupplementarySilverAuditHistory(string customerId)
        {
            string supplementarySilverAuditHistoryUri = $"{ConfigReader.Get(Common.Constants.Environment.AuditMsUrl)}{Common.Constants.Application.AuditMSSupplementarySilverAuditHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);

            var auditMSToken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AuditAppId));

            Dictionary<string, string> auditMSHeaders = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AuditOcpKey) }
            };

            return new SupplementarySilverAuditHistoryResponse()
            {
                SupplementarySilverAuditHistory = (await _connector.GetAsync<SupplementarySilverAuditHistoryResponse>(supplementarySilverAuditHistoryUri, auditMSHeaders, auditMSToken)).Response.SupplementarySilverAuditHistory
            };
        }
    }
}