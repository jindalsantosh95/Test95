using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.Loyalty
{
    [Collection("LoyaltyHealthCheckTests")]
    public class VacTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>

    {
        private readonly CosmosFixture _cosmosFixture;

        private readonly TokenFixture _tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string SingleOrderViewCosmosDb;

        private static string ocpSubKey = string.Empty;

        public VacTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;

            _tokenFixture = tokenFixture;

            customerCosmosDb = Application.CustomerCosmosDbProd;
            customerTransactionCosmosDb = Application.CustomerTransactionCosmosDbProd;
            SingleOrderViewCosmosDb = Application.SingleOrderViewCosmosDbProd;

            var cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            var cosmosEndpoint = Application.CustomerCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            var singleOrderViewCosmosEndpoint = Application.SingleOrderViewCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(singleOrderViewCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-singleOrderView-Automation-Key"));

            _cosmosFixture.RegisterType<CustomerOrderAssociation>(singleOrderViewCosmosEndpoint, SingleOrderViewCosmosDb, Application.CustomerOrderAssociationCollection);

            ocpSubKey = Vault.Default.GetSecretValue("Testing-Ocp-Apim-SubscriptionKey") ?? "";

            _tokenFixture.AddTokenForGraphQlFromKeyVault("GraphQLProd");

        }

        [Trait("Category", GlobalTraits.LoyaltyHealthCheck)]
        [Trait("Tag", GlobalTraits.Loyalty)]
        [Fact]
        public void ValidateOrderForVacMember()
        {
            #region fetching vac accounts from reporting db for future booking validations

            string queryForVacEmailList = "SELECT b.ContactValue FROM [SCV].[FlyingClubMember] a  INNER JOIN [SCV].[ContactDetails] b ON a.customerId = b.customerId  WHERE a.status = 'Active'  AND a.tiercode = 'VAC'  AND a.createdOn BETWEEN DATEADD(minute, -16, GETDATE()) AND DATEADD(minute, -1, GETDATE()) AND b.ContactType = 'Email'  ORDER BY a.createdOn DESC";

            List<string> VacEmailIdList = AzureReportingDBHelper.GetSingleColumnList<string>(queryForVacEmailList);

            // Check if the list is null or empty
            if (VacEmailIdList == null || !VacEmailIdList.Any())
            {
                //  No VAC email records found in the last 15 minutes. Skipping validation.
                return; // Exit the method if no records found
            }

            #endregion

            #region Validations in the single order view DB customerorder Assocation Document

            List<string> emailsWithoutDocs = new(); // To track emails without documents

            foreach (string originalEmail in VacEmailIdList)
            {
                string email = originalEmail.ToUpper();

                string queryForCustomerOrderAssociation = "SELECT TOP 1 * FROM CustomerOrderAssociation c WHERE c.DocType = @docType AND c.CustomerEmail = @emailId ORDER BY c.CreatedOn DESC";

                var query = new QueryDefinition(query: queryForCustomerOrderAssociation).WithParameter("@docType", "CustomerOrderAssociation").WithParameter("@emailId", email); // Pass single emailID

                CustomerOrderAssociation customerOrderAssociationDoc = _cosmosFixture.CosmosQueryItem<CustomerOrderAssociation>(query);

                if (customerOrderAssociationDoc is null)
                {
                    emailsWithoutDocs.Add(email);
                }
            }
            // Assert that all emails have corresponding documents
            Assert.True(emailsWithoutDocs is null || emailsWithoutDocs.Count == 0,
                $"Failed to retrieve CustomerOrderAssociation documents for the following email(s): {string.Join(", ", emailsWithoutDocs)}");
            #endregion
        }
    }
}