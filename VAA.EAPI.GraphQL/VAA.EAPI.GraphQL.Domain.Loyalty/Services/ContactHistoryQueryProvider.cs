using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class ContactHistoryQueryProvider : CustomerTransactionsProviderBase, IContactHistoryQueryProvider
    {
        public ContactHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<ContactHistoryAuditResponse> GetContactHistory(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerContactHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);
            string customerDocUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerDocByCustomerIdEndPoint}".Replace("{customerId}", customerId);

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve ContactHistory
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            List<ContactHistory> contactAudit = (await _connector.GetAsync<ContactHistoryAuditResponse>(uri, headers, token)).Response.ContactHistoryAudit?.ToList();

            // Retrieve Customer Document
            var customerDoc = (await _connector.GetAsync<Customers>(customerDocUri, headers, token)).Response;

            if (customerDoc != null && customerDoc.ContactDetails.Any(x => x.Primary == "Y"))
            {
                foreach (var contact in customerDoc.ContactDetails.Where(x => x.Primary == "Y"))
                {
                    contactAudit.Add((ContactHistory)contact);
                }
            }

            return new ContactHistoryAuditResponse { ContactHistoryAudit = contactAudit };
        }
    }
}
