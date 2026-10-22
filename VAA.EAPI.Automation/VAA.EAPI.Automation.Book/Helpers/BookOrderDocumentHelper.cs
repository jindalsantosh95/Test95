using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Microservices.Order.Search.Domain.BookingDetails.Request;

namespace VAA.EAPI.Automation.Book.Helpers
{
    public class BookOrderDocumentHelper
    {
        private readonly CosmosFixture cosmosFixture = new();

        public BookOrderDocumentHelper()
        {
            string cosmosEndpointOrderDocument = "https://avadeuncmdbsingleorderview.documents.azure.com:443/";
            string cosmosOrderDocumentSecretName = "TESTING-CmdbOrderDocument-Automation-Key";
            string cosmosOrderDocumentKey = Vault.Default.GetSecretValue(cosmosOrderDocumentSecretName);

            if (cosmosOrderDocumentKey.IsNullOrWhiteSpace())
            {
                string keyVaultUrl = ConfigReader.Get(VAA.EAPI.Automation.Core.Constants.Environment.KeyVaultUrl);
                throw new InvalidOperationException(
                    $"Required Key Vault secret '{cosmosOrderDocumentSecretName}' was null/empty. " +
                    $"Cannot validate order document in Cosmos. " +
                    $"Environment='{Config.Environment}', KeyVaultUrl='{keyVaultUrl ?? "<null>"}'.");
            }

            cosmosFixture.RegisterClient(cosmosEndpointOrderDocument, cosmosOrderDocumentKey);

            cosmosFixture.RegisterType<OrderDocument>(cosmosEndpointOrderDocument, !Config.Environment.Is("SIT") ? (!Config.Environment.Is("DEV") ? "SingleOrderViewDB" : "SingleOrderViewDB_Dev") : "SingleOrderViewDB_Test", "OrderCollection");
        }

        public OrderDocument CosmosOrderDocumentCall(string queryForOrderDoc)
        {
            OrderDocument orderDoc = cosmosFixture.QueryItem<OrderDocument>(queryForOrderDoc);

            int retry = 10;
            while (retry > 0 && orderDoc == null)
            {
                Thread.Sleep(5000);
                orderDoc = cosmosFixture.QueryItem<OrderDocument>(queryForOrderDoc);
                retry--;
            }

            Assert.True(orderDoc != null, "Query from Cosmos DB for Order Document is null after 10 re tries");

            return orderDoc;
        }

        public string GetPNRDetails(string PNR)
        {
            string queryForOrderDoc = $"SELECT * FROM c where c.DocType= 'OrderDocument' and c.BookingReference.RLOC = ('{PNR}')";

            OrderDocument OrderDoc = CosmosOrderDocumentCall(queryForOrderDoc);

            if (OrderDoc.BookingReference?.RLOC != null) return OrderDoc.BookingReference.RLOC;

            return null;
        }

        public OrderDocument GetPNRInsuranceAddedDetails(string PNR)
        {
            string queryForOrderDoc = $"SELECT * FROM c where c.DocType= 'OrderDocument' and c.BookingReference.RLOC = ('{PNR}')";

            OrderDocument OrderDoc = CosmosOrderDocumentCall(queryForOrderDoc);
         
                return (OrderDoc != null && OrderDoc.IsInsuranceAdded) ? OrderDoc : null;
        }

        public OrderDocument GetPNRRejectedInsuranceAddedDetails(string PNR)
        {
            string queryForOrderDoc = $"SELECT * FROM c where c.DocType= 'OrderDocument' and c.BookingReference.RLOC = ('{PNR}')";

            OrderDocument OrderDoc = CosmosOrderDocumentCall(queryForOrderDoc);

            return (OrderDoc != null && !OrderDoc.IsInsuranceAdded) ? OrderDoc : null;
        }
    }
}
