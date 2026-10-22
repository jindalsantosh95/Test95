using Azure.Messaging.ServiceBus;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.LtgAndMillionMiller
{
    [Collection("IntegrationTests")]
    public class MillionMillerEventTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        public static string env = ConfigReader.Get("env");

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        public MillionMillerEventTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;


            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);


            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }


        public static string serviceBusConnectionString = Vault.Default.GetSecretValue("MarketingLoyalty-Test-ServiceBusConnectionString");
        public static string serviceBusTopicName = Application.ServiceBusTopicNameLoyaltyMS;
        public static string cloudEventDate;


        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task MillionMiler_Success()
        {
            //Test scenario will trigger an email to the customer and create a txn for 100K for the member
            #region Pre-Requisite for test data creation - searching a customer with Million Miler award document exists and Flown miles of >100K 

            string queryForAwardDocument = $"SELECT Top 1 * FROM  c where c.DocType='Award' and c.ProductId='1634561592' order by c._ts desc";

            Award memberAwardDocument = cosmosFixture.QueryItem<Award>(queryForAwardDocument);

            int retry = 5;

            while (retry > 0 && memberAwardDocument == null)
            {
                await Task.Delay(500);

                memberAwardDocument = cosmosFixture.QueryItem<Award>(queryForAwardDocument);

                retry--;
            }

            string customerId = memberAwardDocument.CustomerId;

            string initiatedBy = "AutomationScript";
            #endregion

            #region Pre-Requisite for updating the emailAddress of the customer to EAPICICDTest@fly.virgin.com

            string queryForCustomerDocument = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{customerId}'";

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

            string memberNumber = customerDocument.LoyaltyMemberNumber;

            //ContactDetails[] contactDetails = customerDocument.ContactDetails;

            //for (int i = 0; i < contactDetails.Length; i++)
            //{
            //    if (contactDetails[i].ContactType == "Email")

            //        customerDocument.ContactDetails[i].ContactValue = Application.EmailInboxForValidations;
            //}

            //Customer scvCustomerDoc = customerDocument;

            //string queryForCustomerSearchDocument = $"SELECT * FROM  c where c.DocType='CustomerSearch' and c.PartitionKey='{memberNumber}'";

            //CustomerSearch customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

            //customerSearchDocument.EmailAddress = Application.EmailInboxForValidations;

            //CustomerSearch scvCustomerSearchDoc = customerSearchDocument;

            //string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, scvCustomerSearchDoc);

            //Assert.True(updatestatus.Equals("OK"),
            //    $"EmailAddress for the member {memberNumber} was not updated to {Application.EmailInboxForValidations} in customer collection DB");

            #endregion

            #region Creation of Cloud Event

            MillionMillerAwardMemberDetails millionMillerAwardMemberDetails = MillionMillerAwardEventMapper.MillionMillerAwardMemberDetails(customerId, memberNumber, initiatedBy);

            CloudEvent cloudEvent = MillionMillerAwardEventMapper.MillionMillerAwardEvent(millionMillerAwardMemberDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new(System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region Email Validations for the MillionMiller Award 

            //string millionMillerSubjectLine = "This is a test, Please ignore - Congrats, you are a million miler- This is Stage Test (Pls Ignore)";

            //string emailContent = EmailValidator.EmailRecievedINInbox(millionMillerSubjectLine);

            //Assert.True(EmailContent.Value[0].Subject.ToString().Contains(millionMillerSubjectLine),
            //     $"Expected Million Miler Email Subject to be {millionMillerSubjectLine} but found {EmailContent.Value[0].Subject}.");

            ////Assert.True(EmailContent.Value[0].UniqueBody.Content.ToString().ToLower().Has($"hello {customerDocument.FirstName.ToLower()}"),
            ////   $"Expected the FirstName in the Email to be {customerDocument.FirstName} But Found incorrect/Different Value.");

            //await EmailValidator.DeleteEmailAsync(EmailContent);

            #endregion

            #region COSMOS Txn validations  for 100k transaction for the member

            string queryForTransactionDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'Transaction' and c.PartitionKey='{customerId}' and c.LastUpdatedOn like '%{cloudEventDate}%'";

            Transaction transaction = cosmosFixture.QueryItem<Transaction>(queryForTransactionDocument);

            string productId = "1679404992929";

            string transactionValue = "100000";

            int retrynew = 10;

            while (retrynew > 0 && transaction == null)
            {
                await Task.Delay(500);

                transaction = cosmosFixture.QueryItem<Transaction>(queryForTransactionDocument);

                retrynew--;
            }

            if (transaction is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is Available for the Searched Query - {queryForTransactionDocument}.");
            }

            Assert.True(customerId.Equals(transaction.CustomerId),
                $"Expected the CustomerId as {customerId}  but found {transaction.CustomerId}");

            Assert.True(memberNumber.Equals(transaction.MemberNumber),
                $"Expected the Member Number as {memberNumber}  but found {transaction.MemberNumber}");

            Assert.True(productId.Equals(transaction.ProductId),
                $"Expected the Million Miler Award product Id in transaction document to be {productId}  but found {transaction.ProductId}");

            Assert.True("Accrual".Equals(transaction.TransactionType),
                $"Expected the Transaction Type as Accrual but found {transaction.TransactionType}");

            Assert.True("Processed".Equals(transaction.StatusCode),
               $"Expected the Transaction Status to be Processed but found {transaction.StatusCode}");

            Assert.True(transactionValue.Equals(transaction.TransactionValue),
               $"Expected the transaction Value as {transactionValue}  but found {transaction.TransactionValue}");

            string createdOnDate = transaction.CreatedOn?.ToString("yyyy-MM-dd");

            Assert.True(cloudEventDate.Equals(createdOnDate),
                $"Expected the date time for event {cloudEventDate}  but found {createdOnDate}");

            #endregion
        }
    }
}
