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
using Customer = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;
using CustomerSearch = VAA.EAPI.Automation.Core.DB_Model.Entities.CustomerSearch;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.HouseHold
{

    [Collection("IntegrationTests")]
    public class HouseHoldDissolveEventTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        public static string env = ConfigReader.Get("env");
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public HouseHoldDissolveEventTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            preRequisiteTestData = new();

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        public static string serviceBusConnectionString = Vault.Default.GetSecretValue("MarketingLoyalty-Test-ServiceBusConnectionString");
        public static string serviceBusTopicName = Application.ServiceBusTopicNameLoyaltyMS;
        public static string cloudEventDate;

        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task HouseholdDissolve_SuccessAsync()
        {

            string parentCustomerId = await preRequisiteTestData.LinkingAChildToAParentAccount();

            #region Searching for the Group Document for the Parent Id

            string queryForGroupDocument = $"SELECT Top 1 * FROM  c where c.DocType='Group' and c.HeadOfGroupCustomerId='{parentCustomerId}' ";

            Group group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

            int retry = 5;

            while (retry > 0 && group == null)
            {
                await Task.Delay(500);

                group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

                retry--;
            }

            if (group is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForGroupDocument}.");
            }

            string groupId = group.GroupId;
            string headOfGroupCustomerId = group.HeadOfGroupCustomerId;
            string initiatedBy = "AutomationScript";
            string etag = group._etag;
            #endregion

            #region Pre-Requisite for updating the emailAddress of the customer to EAPICICDTest@fly.virgin.com

            //string queryForCustomerDocument = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{headOfGroupCustomerId}'";

            //Customer customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            //retry = 5;

            //while (retry > 0 && customerDocument == null)
            //{
            //    await Task.Delay(500);

            //    customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            //    retry--;
            //}

            //if (customerDocument is null)
            //{
            //    throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForCustomerDocument}.");
            //}

            //ContactDetails[] contactDetails = customerDocument.ContactDetails;

            //for (int i = 0; i < contactDetails.Length; i++)
            //{
            //    if (contactDetails[i].ContactType == "Email")

            //        customerDocument.ContactDetails[i].ContactValue = Application.EmailInboxForValidations;
            //}

            //string memberNumber = customerDocument.LoyaltyMemberNumber;

            //Core.DB_Model.Entities.Customer scvCustomeDoc = customerDocument;

            //string queryForCustomerSearchDocument = $"SELECT * FROM  c where c.DocType='CustomerSearch' and c.PartitionKey='{memberNumber}'";

            //CustomerSearch customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

            //customerSearchDocument.EmailAddress = Application.EmailInboxForValidations;

            //CustomerSearch scvCustomerSearchDoc = customerSearchDocument;

            //string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomeDoc, scvCustomerSearchDoc);

            //Assert.True(updatestatus.Equals("OK"),
            //    $"EmailAddress for the member {memberNumber} was not updated to {Application.EmailInboxForValidations} in customer collection DB");

            #endregion


            #region Creation of Cloud Event


            HouseholdDissolveMember memberDelinkDetails = HouseHoldEventMapper.HouseholdMemberDetails(headOfGroupCustomerId, groupId, initiatedBy);

            CloudEvent cloudEvent = HouseHoldEventMapper.MapHouseholdEvent(memberDelinkDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region COSMOS validations if the member got delinked            

            string queryForGroupDocumentValidations = $"SELECT * FROM  c where c.DocType='Group'  and c.Status='Active' and c.GroupId='{groupId}' and c.GroupHistory[0].ExitDate!=''";

            Group groupForValidation = cosmosFixture.QueryItem<Group>(queryForGroupDocumentValidations);

            int retryAgain = 5;

            while (retryAgain > 0 && (groupForValidation == null || etag == groupForValidation._etag))
            {
                await Task.Delay(500);

                groupForValidation = cosmosFixture.QueryItem<Group>(queryForGroupDocumentValidations);

                retryAgain--;
            }

            if (groupForValidation is null || etag == groupForValidation._etag)
            {
                throw new Xunit.Sdk.XunitException($"Document Not available in Cosmos with searched Query{queryForGroupDocumentValidations} for the GroupId {groupId}. The ExitDate is not updated after delinking.");
            }

            int count = 0;

            foreach (GroupHistory history in groupForValidation.GroupHistory)
            {
                Assert.True(!string.IsNullOrEmpty(history.ExitDate),
                    "The Household dissolve did not work for all the associated child Id's");
                if (history.ExitDate.Has(cloudEventDate))
                {
                    count += 1;
                }

            }

            Assert.True(count >= 1, $"Expected atleast one history with ExitDate of child as current date but the count was {count}.");


            #endregion            

            #region Email Validations for the Household Dissolve :  

            //string delinkEmailSubjectLine = "Important Information about your Flying Club account- This is Stage Test (Pls Ignore)";

            //string emailContent = EmailValidator.EmailRecievedINInbox(delinkEmailSubjectLine);

            //Assert.True(EmailContent.Value[0].Subject.ToString().Equals(delinkEmailSubjectLine),
            //     $"Expected De link email Subject to be {delinkEmailSubjectLine} but found {EmailContent.Value[0].Subject}.");

            ////Assert.True(EmailContent.Value[0].UniqueBody.Content.ToString().ToLower().Has($"hello {customerDocument.FirstName.ToLower()}"),
            ////   $"Expected the FirstName in the Email to be {customerDocument.FirstName} But Found incorrect/Different Value.");

            //await EmailValidator.DeleteEmailAsync(EmailContent);

            #endregion


        }

        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task HouseholdDissolveOnMemberStatusUpdate_SuccessAsync()
        {

            string parentCustomerId = await preRequisiteTestData.LinkingAChildToAParentAccount();

            #region Searching for the Group Document for the Parent Id 

            string queryForGroupDocument = $"SELECT Top 1 * FROM  c where c.DocType='Group' and c.HeadOfGroupCustomerId='{parentCustomerId}' ";

            Group group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

            int retry = 5;

            while (retry > 0 && group == null)
            {
                await Task.Delay(500);

                group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

                retry--;
            }

            if (group is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForGroupDocument}.");
            }

            string groupId = group.GroupId;
            string headOfGroupCustomerId = group.HeadOfGroupCustomerId;
            string initiatedBy = "AutomationScript";
            string etag = group._etag;
            #endregion

            #region Pre-Requisite for updating the emailAddress of the customer to EAPICICDTest@fly.virgin.com

            //string queryForCustomerDocument = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{headOfGroupCustomerId}'";

            //Customer customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            //retry = 5;

            //while (retry > 0 && customerDocument == null)
            //{
            //    await Task.Delay(500);

            //    customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            //    retry--;
            //}

            //if (customerDocument is null)
            //{
            //    throw new Xunit.Sdk.XunitException($"No Document is available for the searched Query {queryForCustomerDocument}.");
            //}

            //ContactDetails[] contactDetails = customerDocument.ContactDetails;

            //for (int i = 0; i < contactDetails.Length; i++)
            //{
            //    if (contactDetails[i].ContactType == "Email")

            //        customerDocument.ContactDetails[i].ContactValue = Application.EmailInboxForValidations;
            //}

            //string memberNumber = customerDocument.LoyaltyMemberNumber;

            //Customer scvCustomeDoc = customerDocument;

            //string queryForCustomerSearchDocument = $"SELECT * FROM  c where c.DocType='CustomerSearch' and c.PartitionKey='{memberNumber}'";

            //CustomerSearch customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

            //customerSearchDocument.EmailAddress = Application.EmailInboxForValidations;

            //CustomerSearch scvCustomerSearchDoc = customerSearchDocument;

            //string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomeDoc, scvCustomerSearchDoc);

            //Assert.True(updatestatus.Equals("OK"),
            //    $"EmailAddress for the member {memberNumber} was not updated to {Application.EmailInboxForValidations} in customer collection DB");

            #endregion

            #region  Fetching member details from FCM Document

            string queryForFlyingClubDocument = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember' and c.PartitionKey='{headOfGroupCustomerId}'";

            FlyingClubMember memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

            retry = 5;

            while (retry > 0 && memberProfileDocument == null)
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

                retry--;
            }

            string memberType = memberProfileDocument.MemberType;

            Assert.True(memberType.Equals("Household"),
                $"Expected the member type to be Household but found {memberType}.");

            string oldStatus = memberProfileDocument.Status;

            Random random = new();

            //List of new status values for which household auto dissolve is expected
            List<string> dissolveStatusList = new()
            { "Inactive", "Fraud", "Active Pending", "Deceased", "Expired", "Cancelled" };

            string newStatus = dissolveStatusList[random.Next(dissolveStatusList.Count)];

            #endregion

            #region Creation of Cloud Event


            HouseholdDissolveMemberOnStatusUpdate dissolveMemberDetails = HouseHoldDissolveEventMapper.HouseholdDissolveMemberDetails(headOfGroupCustomerId, oldStatus, newStatus, memberType, "", groupId, initiatedBy);

            CloudEvent cloudEvent = HouseHoldDissolveEventMapper.MapHouseholdDissolveEvent(dissolveMemberDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new
                (System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region COSMOS validations if the member got delinked


            string queryForGroupDocumentValidations = $"SELECT * FROM  c where c.DocType='Group'  and c.Status='Active' and c.GroupId='{groupId}' and c.GroupHistory[0].ExitDate!=''";

            Group groupForValidation = cosmosFixture.QueryItem<Group>(queryForGroupDocumentValidations);

            int retryAgain = 5;

            while (retryAgain > 0 && (groupForValidation == null || etag == groupForValidation._etag))
            {
                await Task.Delay(500);

                groupForValidation = cosmosFixture.QueryItem<Group>(queryForGroupDocumentValidations);

                retryAgain--;
            }

            if (groupForValidation is null || etag == groupForValidation._etag)
            {
                throw new Xunit.Sdk.XunitException($"Document Not available in Cosmos with searched Query{groupForValidation} for the GroupId {groupId}. " +
                    $"The ExitDate is not updated after delinking.");
            }

            int count = 0;

            foreach (GroupHistory history in groupForValidation.GroupHistory)
            {
                Assert.True(!string.IsNullOrEmpty(history.ExitDate),
                    "The Household dissolve did not work for all the associated child Id's");

                if (history.ExitDate.Has(cloudEventDate))
                {
                    count += 1;
                }

            }

            Assert.True(count >= 1, $"Expected atleast one history with ExitDate of child as current date but the count was {count}.");

            #endregion

            #region Email Validations for the Household Dissolve :  

            //string delinkEmailSubjectLine = "Important Information about your Flying Club account- This is Stage Test (Pls Ignore)";

            //string emailContent = EmailValidator.EmailRecievedINInbox(delinkEmailSubjectLine);

            //Assert.True(EmailContent.Value[0].Subject.ToString().Equals(delinkEmailSubjectLine),
            //     $"Expected De link email Subject to be {delinkEmailSubjectLine} but found {EmailContent.Value[0].Subject}.");

            ////Assert.True(EmailContent.Value[0].UniqueBody.Content.ToString().ToLower().Has($"hello {customerDocument.FirstName.ToLower()}"),
            ////   $"Expected the FirstName in the Email to be {customerDocument.FirstName} But Found incorrect/Different Value.");

            //await EmailValidator.DeleteEmailAsync(EmailContent);

            #endregion



        }
    }
}
