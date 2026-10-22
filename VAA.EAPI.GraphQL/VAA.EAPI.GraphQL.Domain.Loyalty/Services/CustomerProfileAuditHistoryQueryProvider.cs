using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerProfileAuditHistoryQueryProvider : CustomerTransactionsProviderBase, ICustomerProfileAuditHistoryQueryProvider
    {
        public CustomerProfileAuditHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<CustomerProfileAuditHistoryResponse> GetCustomerProfileAuditHistory(string customerId)
        {
            string customerProfileAuditHistoryUri = $"{ConfigReader.Get(Common.Constants.Environment.AuditMsUrl)}{Common.Constants.Application.AuditMSCustomerProfileAuditHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);

            var auditMSToken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AuditAppId));

            Dictionary<string, string> auditMSHeaders = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AuditOcpKey) }
            };

            return new CustomerProfileAuditHistoryResponse()
            {
                CustomerProfileHistoryAudit = (await _connector.GetAsync<CustomerProfileAuditHistoryResponse>(customerProfileAuditHistoryUri, auditMSHeaders, auditMSToken)).Response.CustomerProfileHistoryAudit
            };
        }
    }
}