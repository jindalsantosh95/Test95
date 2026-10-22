using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.TestDataCreation
{
    public class CreateBulkTransaction : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>, IClassFixture<CreateTransactionHelper>
    {

        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;
        private readonly CreateTransactionHelper _createTransactionHelper;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        private readonly string currentWorkingDirectory = Path.GetFullPath(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName);

        public CreateBulkTransaction(TokenFixture tokenFixture, CosmosFixture cosmosFixture, CreateTransactionHelper createTransactionHelper)
        {
            _cosmosFixture = cosmosFixture;
            _tokenFixture = tokenFixture;
            _createTransactionHelper = createTransactionHelper;

            if (!backendAPIEnv.Is("Project-Dev"))
                _tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphql");
            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");
        }

        [Trait("Category", "Local-Run")]
        [Theory]
        [InlineData(Application.TransactionProduct.Flight)]
        public async Task CreateTransactionSuccessScenario(Application.TransactionProduct product)
        {

            int count = 5;
            List<string> txnId = new List<string>();

            while (count > 0) // Create 25 flight accrual transaction
            {

                #region Create Transaction
                // Define the partner code
                string partnerCode = "VS";
                // Generate a random booking reference
                string bookingReference = DataGenerator.RandomString(6, true).ToUpper();
                // Create a transaction using the helper method
                CreateTransactionResponse createTransactionResponse = await _createTransactionHelper.CreateTransaction(
                    product, partnerCode, bookingReference, DateTime.Today, DateTime.Today, 10000, 0, "", "");

                txnId.Add(createTransactionResponse.transactionId);
                count--;

                #endregion

            }
            if (txnId.Count == 0)
            {

                Assert.Fail("Transaction list is empty. No transactions were created.");
            }
            else
            {
                string fileName = "TransactionIDs.txt";
                string filePath = $"{currentWorkingDirectory}\\{fileName}";


                await File.WriteAllLinesAsync(filePath, txnId);
            }

        }

    }
}
