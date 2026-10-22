using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    public class CancelTransactionTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>, IClassFixture<CreateTransactionHelper>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;
        private readonly CreateTransactionHelper _createTransactionHelper;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public CancelTransactionTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture, CreateTransactionHelper createTransactionHelper)
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

        [Theory]
        [InlineData(Application.TransactionProduct.Flight)]
        public async Task CancelRelatedTransactionSuccessScenario(Application.TransactionProduct product)
        {
            #region Create Transaction
            // Define the partner code
            string partnerCode = "VS";
            // Generate a random booking reference
            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();
            // Create a transaction using the helper method
            CreateTransactionResponse createTransactionResponse = await _createTransactionHelper.CreateTransaction(
                product, partnerCode, bookingReference, DateTime.Today, DateTime.Today, 10000, 0, "", "");

            #endregion

            #region Retrieve Related Transaction

            HttpResponseMessage relatedTxnResponse = GraphQL.RetrieveRelatedTransactions(_tokenFixture.GetToken("graphql"), createTransactionResponse.customerId, createTransactionResponse.memberNumber, createTransactionResponse.transactionId);

            string responseContent = await relatedTxnResponse.Content.ReadAsStringAsync();

            RetrieveRelatedTxnGqlResponse relatedTxnResponseContent = JsonConvert.DeserializeObject<RetrieveRelatedTxnGqlResponse>(responseContent);

            Assert.True(relatedTxnResponseContent?.data?.loyaltyRelatedTransactions is not null, $"Expected a valid response but the returned value is \n {responseContent}");

            List<Canceltransactiondetail>
                cancelTransactionDetails = MapRelatedTransactionDetailsToCancelTransactionRequest(relatedTxnResponseContent.data.loyaltyRelatedTransactions.transactionDocuments, createTransactionResponse);

            #endregion

            string cancellationReason = "Canx Agent Error";

            string initiatedBy = Application.AutomationScriptCreatedBy;

            #region Cancel Transaction

            await Task.Delay(2000); // Adding a delay to ensure the transaction is processed before cancellation

            HttpResponseMessage cancelTxnResponse = GraphQL.CancelRelatedTransactions(_tokenFixture.GetToken("graphql"), createTransactionResponse.customerId, createTransactionResponse.memberNumber, createTransactionResponse.transactionId, cancellationReason, initiatedBy, cancelTransactionDetails);

            string cancellationResponse = await cancelTxnResponse.Content.ReadAsStringAsync();

            string expectedCancelTxnResponse = "Cancellation Request Processed successfully";

            Assert.True(cancellationResponse.Has(expectedCancelTxnResponse), $"Expected a successful response but the returned value is \n {cancellationResponse} for the member {createTransactionResponse.memberNumber}");

            #endregion
        }

        public static List<Canceltransactiondetail> MapRelatedTransactionDetailsToCancelTransactionRequest(TransactionDocuments[] transactionDocuments, CreateTransactionResponse createTransactionResponse)
        {

            List<Canceltransactiondetail> transactions = new();

            transactions.Add(
                  new()
                  {
                      transactionId = createTransactionResponse.transactionId,
                      transctionType = createTransactionResponse.transactionType,
                      transactionStatus = createTransactionResponse.transactionStatus,
                      productName = createTransactionResponse.transactionProduct.ToString(),
                      partnerId = createTransactionResponse.partnerId,
                      productId = string.Empty,
                      partnerName = string.Empty,
                  }
              );

            foreach (TransactionDocuments transactionDocument in transactionDocuments)
            {
                transactions.Add(new()
                {
                    transactionId = transactionDocument.transactionId,
                    transctionType = transactionDocument.transactionType,
                    transactionStatus = transactionDocument.statusCode,
                    partnerId = transactionDocument.partnerId,
                    productId = transactionDocument.productId
                });
            }

            return transactions;
        }
    }
}
