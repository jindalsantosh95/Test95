using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public class UpdateCosmosAttribute
    {
        private static readonly object _clientLock = new object();

        private static DocumentClient _client;

        private static DocumentClient DocumentClient
        {
            get
            {
                if (_client == null)
                {
                    lock (_clientLock)
                    {
                        _client ??= new DocumentClient(new Uri(Application.CustomerCosmosDbEndpoint), Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
                    }
                }

                return _client;
            }
        }
        public static DocumentClient GetDocumentClient()
        {
            return DocumentClient;
        }

        public static async Task<string> UpdateAttributeInCustomerCollectionDB(Customer scvCustomerDoc, CustomerSearch scvCustomerSearchDoc)
        {
            string env = Config.Environment;

            string customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            string updatestatus = "";

            IDocumentClient _memClient;
            //CustomerDocument Update
            if (scvCustomerDoc != null)
            {
                _memClient = GetDocumentClient();

                ResourceResponse<Document> updateresponse = await _memClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri(customerCosmosDb, Application.CustomerCosmosCollection, scvCustomerDoc.Id), scvCustomerDoc);

                updatestatus = updateresponse.StatusCode.ToString();
            }
            //CustomerSearchDocument Update
            if (scvCustomerSearchDoc != null && updatestatus.Equals("OK"))
            {
                _memClient = GetDocumentClient();

                ResourceResponse<Document> updateresponse = await _memClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri(customerCosmosDb, Application.CustomerCosmosCollection, scvCustomerSearchDoc.Id), scvCustomerSearchDoc);

                updatestatus = updateresponse.StatusCode.ToString();
            }

            return updatestatus;
        }
    }
}

