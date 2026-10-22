using Azure.Messaging.ServiceBus;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.CampaignManagement
{
    [Collection("IntegrationTests")]
    public class CampaignRegisterEventTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public static string env = ConfigReader.Get("env");

        public CampaignRegisterEventTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        public static string serviceBusConnectionString = Vault.Default.GetSecretValue("MarketingLoyalty-Test-ServiceBusConnectionString");
        public static string serviceBusTopicName = Application.ServiceBusTopicNameCampaignMS;
        public static string cloudEventDate;

        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task CampaignRegisterEvent_Success()
        {

            #region Pre-Requisite for test data creation - searching a TargetedMembership Doc

            string queryForTargetedMembershipDocument = $"SELECT Top 1 * FROM  c where c.DocType='TargetedMembership'  and c.StatusCode='Targeted' order by c.LastUpdatedOn desc";

            TargetedMembership targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            int retry = 5;

            while (retry > 0 && targetedMembership == null)
            {
                await Task.Delay(500);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            if (targetedMembership is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForTargetedMembershipDocument}.");
            }

            string customerId = targetedMembership.CustomerId;

            string campaignId = targetedMembership.ProfilingCampaignSourceId;

            string emailId = Application.EmailInboxForValidations;

            string documentId = targetedMembership.Id;

            string etag = targetedMembership._etag;

            string createdBy = "AutomationScript";

            string documentStatus = "Registered";

            string queryForCustomerDocument = $"SELECT Top 1 * FROM  c where c.DocType='Customer'  and c.PartitionKey='{customerId}' order by c.LastUpdatedOn desc";

            Customer customerDoc = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            retry = 5;

            while (retry > 0 && customerDoc == null)
            {
                await Task.Delay(500);

                customerDoc = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

                retry--;
            }

            if (customerDoc is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForCustomerDocument}.");
            }

            string firstName = customerDoc.FirstName;

            string lastName = customerDoc.LastName;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string queryForCampaignDetails = $"Select CampaignCode,Type,EndDate from dbo.Campaigns where RowId='{campaignId}'";

            object[][] response = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignCode = response[0][0]?.ToString();

            string campaignType = response[0][1]?.ToString();

            string campaignEndDate = response[0][2]?.ToString();

            #endregion

            #region Creation of Cloud Event

            CampaignRegister campaignRegisterDetails = CampaignRegisterEventMapper.CampaignRegisterDetails(customerId, campaignCode, campaignId, createdBy, emailId, firstName, lastName, campaignType, documentId, documentStatus, memberNumber, campaignEndDate);

            CloudEvent cloudEvent = CampaignRegisterEventMapper.MapCampaignRegisterEvent(campaignRegisterDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new(System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region validation of document status after registration            

            queryForTargetedMembershipDocument = $"SELECT * FROM  c where c.DocType='TargetedMembership'  and c.id='{documentId}' order by c.LastUpdatedOn desc";

            targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            retry = 5;

            while (retry > 0 && (targetedMembership == null || etag == targetedMembership._etag))
            {
                await Task.Delay(500);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            if (targetedMembership is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForTargetedMembershipDocument}.");
            }

            Assert.True(targetedMembership.StatusCode.Equals("Registered"),
                $"Expected the Document Status to be Registered after the Registration but found {targetedMembership.StatusCode}.");

            Assert.True(targetedMembership.AssignmentDate.Value.DateTime.ToString("yyyy-MM-dd").Equals(cloudEventDate),
                $"Expected the Assignment Date after the Registration to be {cloudEventDate} but found {targetedMembership.AssignmentDate.Value.DateTime.ToString("yyyy-MM-dd")}.");

            #endregion


            #region Email Validations for the Campaign Register Email :  

            //if (!campaignType.Equals("Flying Club Communication(CMD)") && !campaignType.Equals("Other")
            //    && !campaignType.Equals("Web Announcements"))
            //{

            //    string campaignRegisterEmailSubjectLine = "You're all registered- This is Stage Test (Pls Ignore)";

            //    string emailContent = EmailValidator.EmailRecievedINInbox(campaignRegisterEmailSubjectLine);

            //    Assert.True(EmailContent.Value[0].Subject.ToString().Equals(campaignRegisterEmailSubjectLine),
            //         $"Expected Campaign Register email Subject to be {campaignRegisterEmailSubjectLine} but found {EmailContent.Value[0].Subject}.");

            //    Assert.True(EmailContent.Value[0].UniqueBody.Content.ToString().ToLower().Has($"hello {customerDoc.FirstName.ToLower()}"),
            //       $"Expected the FirstName in the Email to be {customerDoc.FirstName} But Found incorrect/Different Value.");

            //    await EmailValidator.DeleteEmailAsync(EmailContent);
            //}

            #endregion

        }
    }
}
