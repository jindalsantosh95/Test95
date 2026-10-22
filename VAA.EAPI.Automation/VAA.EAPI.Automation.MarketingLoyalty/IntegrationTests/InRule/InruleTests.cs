using Azure.Messaging.ServiceBus;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.InRule
{
    [Collection("IntegrationTests")]
    public class InruleTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        public static string env = ConfigReader.Get("env");
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public InruleTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }


        public static string serviceBusConnectionString = Vault.Default.GetSecretValue("MarketingLoyalty-Test-ServiceBusConnectionString");
        public static string serviceBusTopicName = Application.ServiceBusTopicNameRuleEngine;

        public static string cloudEventDate;

        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task CreateBirthdayBonusTransaction_SuccessAsync()
        {
            string productName = "Birthday Bonus";

            string productId = "1-1Z08";

            int retry;

            string date = DateTime.UtcNow.AddYears(-18).ToString("yyyy-MM-dd");

            string queryForCustomerDocument = $"SELECT TOP 1 * FROM  c where c.DocType='Customer' and c.DateOfBirth like '%{date}%'";

            Customer customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            retry = 5;

            while (retry > 0 && customerDocument == null)
            {
                await Task.Delay(500);

                customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

                retry--;
            }

            if (customerDocument is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForCustomerDocument}.");
            }

            string customerId = customerDocument.CustomerId;

            string memberNumber = customerDocument.LoyaltyMemberNumber;

            string memberStatus = customerDocument.Status;

            #region Birthday Bonus cloud event creation

            BirthdayBonusMemberDetails memberBirthdayDetails = InruleEventMapper.MemberDetails(customerId, productName, memberNumber, productId, memberStatus);

            CloudEvent cloudEvent = InruleEventMapper.MapBirthdayBonusRuleSetEvent(memberBirthdayDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region COSMOS Txn validations  

            string queryForTransactionDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'Transaction' and c.PartitionKey='{customerId}' and c.CreatedOn like '%{cloudEventDate}%'";

            Transaction transaction = cosmosFixture.QueryItem<Transaction>(queryForTransactionDocument);

            string transactionValue = "2000";

            retry = 10;

            while (retry > 0 && transaction == null)
            {
                await Task.Delay(500);

                transaction = cosmosFixture.QueryItem<Transaction>(queryForTransactionDocument);

                retry--;
            }

            if (transaction is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document found for the Searched Query {queryForTransactionDocument}");
            }

            Assert.True(customerId.Equals(transaction.CustomerId),
                $"Expected the CustomerId as {customerId}  but found {transaction.CustomerId}");

            Assert.True(memberNumber.Equals(transaction.MemberNumber),
                $"Expected the Member Number as {memberNumber}  but found {transaction.MemberNumber}");

            Assert.True(productId.Equals(transaction.ProductId),
                $"Expected the product Id of Birthday Bonus in Transaction Document as {productId}  but found {transaction.ProductId}");

            Assert.True(transactionValue.Equals(transaction.TransactionValue),
                $"Expected the transaction value as {transactionValue}  but found {transaction.TransactionValue}");

            Assert.True("Accrual".Equals(transaction.TransactionType),
               $"Expected the Transaction Type as Accrual but found {transaction.TransactionType}");

            Assert.True("Processed".Equals(transaction.StatusCode),
               $"Expected the Transaction Status to be Processed but found {transaction.StatusCode}");

            string createdOnDate = transaction.CreatedOn?.ToString("yyyy-MM-dd");

            Assert.True(cloudEventDate.Equals(createdOnDate),
                $"Expected the date time for event {cloudEventDate}  but found {createdOnDate}");

            #endregion
        }
    }
}




