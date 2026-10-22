using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper;
using ContactDetails = VAA.EAPI.Automation.Core.DB_Model.Entities.ContactDetails;
using Customer = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;
using CustomerSearch = VAA.EAPI.Automation.Core.DB_Model.Entities.CustomerSearch;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.LtgAndMillionMiller
{

    [Collection("IntegrationTests")]
    public class LtgEmailEventTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        public static string env = ConfigReader.Get("env");

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public LtgEmailEventTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);


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
        public async Task LifeTimeGoldEmailTrigger_Success()
        {
            #region Pre-Requisite for test data creation - searching a customer with Current Tier as Gold

            string queryForFlyingClubDocument = "SELECT Top 1 * FROM  c where c.DocType=@DocType and c.TierCode='Gold' and c.Status='Active' order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFlyingClubDocument).WithParameter("@DocType", "FlyingClubMember");

            FlyingClubMember memberProfileDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retry = 5;

            while (retry > 0 && memberProfileDocument == null)
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retry--;
            }

            string customerId = memberProfileDocument.CustomerId;
            string memberNumber = memberProfileDocument.MemberNumber;
            string initiatedBy = "AutomationScript";
            #endregion

            #region Pre-Requisite for updating the emailAddress of the customer to EAPICICDTest@fly.virgin.com

            string queryForCustomerDocument = "SELECT * FROM  c where c.DocType=@DocType and c.PartitionKey=@CustomerId";

            parameterizedQuery = new QueryDefinition(query: queryForCustomerDocument).WithParameter("@DocType", "Customer").WithParameter("@CustomerId", customerId);

            Customer customerDocument = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            retry = 5;

            while (retry > 0 && customerDocument == null)
            {
                await Task.Delay(500);

                customerDocument = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

                retry--;
            }

            Assert.True(customerDocument is not null,
                $"No Document is available for the searched Query {queryForCustomerDocument}.");

            ContactDetails? contactDetails = customerDocument.ContactDetails?.FirstOrDefault(x => x.ContactType.Equals("Email"));

            Assert.True(contactDetails is not null,
                    $"No Contact Details with type Email found for the customer {customerId}");

            contactDetails.ContactValue = Application.EmailInboxForValidations;

            contactDetails.Primary = "Y";

            Customer scvCustomerDoc = customerDocument;

            string queryForCustomerSearchDocument = $"SELECT * FROM  c where c.DocType='CustomerSearch' and c.PartitionKey='{memberNumber}'";

            CustomerSearch customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

            retry = 5;

            while (retry > 0 && customerSearchDocument == null)
            {
                await Task.Delay(500);

                customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

                retry--;
            }

            customerSearchDocument.EmailAddress = Application.EmailInboxForValidations;

            CustomerSearch scvCustomerSearchDoc = customerSearchDocument;

            string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, scvCustomerSearchDoc);

            Assert.True(updatestatus.Equals("OK"),
                $"EmailAddress for the member {memberNumber} was not updated to {Application.EmailInboxForValidations} in customer collection DB");

            #endregion

            #region Creation of Cloud Event

            Requests.Models.GraphQL.LtgAwardMemberDetails ltgAwardMemberDetails = LtgAwardEventMapper.LtgAwardMemberDetails(customerId, memberNumber, initiatedBy);

            CloudEvent cloudEvent = LtgAwardEventMapper.LtgAwardEvent(ltgAwardMemberDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new(System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region Email Validations for the LTG Award:  

            string ltgSubjectLine = "THIS IS A STAGE TEST! PLEASE IGNORE - Welcome to Lifetime Gold";

            #endregion

        }
    }
}
